using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.Commands;
using static CodeBox.Commands.ActionResults;

namespace CodeBox
{
    public sealed class CommandManager
    {
        private readonly Dictionary<Type, ICommand> commands;
        private readonly Dictionary<CommandKeys, List<ICommand>> commandsByKeys;
        private int counter;
        private bool undoGroup;
        private readonly Editor editor;
        private volatile EditorLock editorLock;
        private bool atomic;

        private sealed class EditorLock : IEditorLock
        {
            private readonly CommandManager man;
            internal volatile int RefCount;

            internal EditorLock(CommandManager man)
            {
                this.man = man;
            }

            public void Release()
            {
                if (--RefCount == 0)
                    man.editorLock = null;
            }
        }

        internal CommandManager(Editor editor)
        {
            commands = new Dictionary<Type, ICommand>();
            commandsByKeys = new Dictionary<CommandKeys, List<ICommand>>();
            this.editor = editor;
        }

        public IEditorLock ObtainLock()
        {
            if (editorLock == null)
                return editorLock = new EditorLock(this);
            else
            {
                editorLock.RefCount++;
                return editorLock;
            }
        }

        public bool BeginUndoAction()
        {
            if (!undoGroup)
            {
                counter++;
                undoGroup = true;
                return true;
            }

            return false;
        }

        public void EndUndoAction() => undoGroup = false;

        private void AddCommand(ICommand cmd) =>
            editor.Buffer.UndoStack.Push(new CommandInfo { Id = counter, Command = cmd });

        public void Undo()
        {
            if (editor.Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(editor.Buffer.UndoStack.Peek().Id, out count, out pos);
                SetEditLines();
                DoAftermath(exp, count, pos);
            }
        }

        private ActionResults Undo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;

            while (editor.Buffer.UndoStack.Count > 0)
            {
                var cmd = editor.Buffer.UndoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = editor.Context;
                    Pos p;
                    var e = cmd.Command.Undo(out p);

                    if (pos.IsEmpty || p < pos)
                        pos = p;

                    exp |= e;

                    if (e.Has(RestoreCaret))
                        AttachCaret(p);

                    editor.Buffer.RedoStack.Push(editor.Buffer.UndoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            return exp;
        }

        public void Redo()
        {
            if (editor.Buffer.RedoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Redo(editor.Buffer.RedoStack.Peek().Id, out count, out pos);
                SetEditLines();
                DoAftermath(exp, count, pos);
            }
        }

        private ActionResults Redo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;

            while (editor.Buffer.RedoStack.Count > 0)
            {
                var cmd = editor.Buffer.RedoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = editor.Context;
                    Pos p;
                    var e = cmd.Command.Redo(out p);
                    pos = p;
                    exp |= e;

                    if (e.Has(RestoreCaret))
                        AttachCaret(p);

                    editor.Buffer.UndoStack.Push(editor.Buffer.RedoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            return exp;
        }

        private void SetEditLines()
        {
            FirstEditLine = int.MaxValue;
            LastEditLine = 0;

            foreach (var s in editor.Buffer.Selections)
            {
                var ln = s.GetFirstLine();

                if (ln < FirstEditLine)
                    FirstEditLine = ln;

                ln = s.GetLastLine();

                if (ln > LastEditLine)
                    LastEditLine = ln;
            }
        }

        public bool Bind<T>(Keys keys) where T : ICommand, new() => Bind<T>(MouseEvents.None, keys);

        public bool Bind<T>(MouseEvents mouse, Keys keys) where T : ICommand, new()
        {
            var type = typeof(T);
            var ci = new T();
            var ck = new CommandKeys(mouse, keys);
            List<ICommand> cl;

            if (!commandsByKeys.TryGetValue(ck, out cl))
            {
                cl = new List<ICommand>();
                commandsByKeys.Add(ck, cl);
            }

            cl.Add(ci);
            return true;
        }

        public void Run(Keys keys) => Run(MouseEvents.None, keys);

        public void Run(MouseEvents mouse, Keys keys)
        {
            List<ICommand> seq;

            if (commandsByKeys.TryGetValue(new CommandKeys(mouse, keys), out seq))
                foreach (var ci in seq)
                    Run(ci.Clone());
        }

        public void Run(ICommand cmd)
        {
            FirstEditLine = int.MaxValue;
            LastEditLine = 0;
            var lines = editor.Lines;
            var modify = cmd is IModifyContent;
            var saveCmd = cmd;

            if (modify && (editor.ReadOnly || editorLock != null))
                return;

            var selCount = editor.Buffer.Selections.Count;
            var qry = selCount == 1 ? null
                : editor.Buffer.Selections.OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var exp = None;
            var thisUndo = false;
            var lastSel = editor.Buffer.Selections.Main;

            if (qry == null)
            {
                FirstEditLine = lastSel.GetFirstLine();
                LastEditLine = lastSel.GetLastLine();
                //cmd = cmd.Clone();
                cmd.Context = editor.Context;
                exp = cmd.Execute(lastSel);

                if (!atomic || !exp.Has(AtomicChange))
                    thisUndo = BeginUndoAction();

                if (exp.Has(Modify))
                    AddCommand(cmd);

                if (exp.Has(RestoreCaret))
                    AttachCaret(lastSel.Caret);
            }
            else
            {
                thisUndo = BeginUndoAction();
                var cc = 0;

                foreach (var sel in qry)
                {
                    var fel = sel.GetFirstLine();

                    if (fel < FirstEditLine)
                        FirstEditLine = fel;

                    var lel = sel.GetLastLine();

                    if (lel > LastEditLine)
                        LastEditLine = lel;

                    cmd.Context = editor.Context;
                    var e = cmd.Execute(sel);

                    exp |= e;

                    if (e.Has(Modify))
                        AddCommand(cmd);

                    if (e.Has(RestoreCaret))
                        AttachCaret(sel.Caret);

                    if (e.Has(SingleRun))
                        break;

                    lastSel = sel;

                    if (++cc < selCount)
                        cmd = cmd.Clone();
                }
            }

            if (thisUndo)
                EndUndoAction();

            if (exp != None)
                DoAftermath(exp, editor.Buffer.Selections.Count, lastSel.Caret);

            if (!exp.Has(IdleCaret))
                editor.MatchBrakets.Match();

            if (exp.Has(AutocompleteKeep) && editor.Autocomplete.WindowShown)
                editor.Autocomplete.UpdateAutocomplete();
            else if (!exp.Has(AutocompleteShow) && editor.Autocomplete.WindowShown)
                editor.Autocomplete.HideAutocomplete();

            atomic = qry == null && exp.Has(AtomicChange);
        }

        private void AttachCaret(Pos pos)
        {
            var line = editor.Document.Lines[pos.Line];

            if (line.Length > pos.Col)
            {
                var ch = line.CharacterAt(pos.Col).WithCaret();
                line[pos.Col] = ch;
            }
            else
                line.TrailingCaret = true;
        }

        private void SetCarets(int count, Pos pos)
        {
            var sels = editor.Buffer.Selections;
            sels.Clear();

            for (var i = pos.Line; i < editor.Document.Lines.Count; i++)
            {
                var line = editor.Document.Lines[i];

                if (line.TrailingCaret)
                {
                    sels.Add(new Selection(new Pos(i, line.Length)));
                    line.TrailingCaret = false;
                    count--;

                    if (count == 0)
                        return;

                    continue;
                }

                for (var j = 0; j < line.Length; j++)
                {
                    var c = line[j];

                    if (c.HasCaret)
                    {
                        sels.Add(new Selection(new Pos(i, j)));
                        line[j] = c.WithoutCaret();
                        count--;

                        if (count == 0)
                            return;
                    }
                }
            }
        }

        private void DoAftermath(ActionResults exp, int selCount, Pos caret)
        {
            var scrolled = false;

            if (exp.Has(Modify))
            {
                editor.Scroll.InvalidateLines(
                    exp.Has(AtomicChange) ? ScrollingManager.InvalidateFlags.Atomic
                    : ScrollingManager.InvalidateFlags.None);

                if (editor.Scroll.Y + editor.Info.TextHeight < -editor.Scroll.YMax)
                    exp |= Scroll;

                if (!exp.Has(ShallowChange))
                    editor.Buffer.Edits++;
            }

            if (exp.Has(RestoreCaret))
                SetCarets(selCount, caret);

            if (exp.Has(Scroll))
            {
                editor.Scroll.SuppressOnScroll = true;
                scrolled = editor.Scroll.UpdateVisibleRectangle();
                editor.Scroll.SuppressOnScroll = false;
            }

            if (scrolled  || exp.Has(Modify))
                editor.Styles.Restyle();

            if (!exp.Has(Silent))
                editor.Redraw();

            if (exp.Has(Modify))
                editor.Folding.RebuildFolding();

            if (exp.Has(LeaveEditor))
            {
                editor.LastKeys = Keys.None;
                editor.Mouse = MouseEvents.None;
                editor.Buffer.Selections.Truncate();
            }
        }

        internal int FirstEditLine { get; private set; }

        internal int LastEditLine { get; private set; }
    }
}
