using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox
{
    internal sealed class CommandManager
    {
        private readonly Dictionary<Type, CommandInfo> commands;
        private readonly Dictionary<Keys, CommandInfo> commandsByKeys;
        private int counter;
        private bool undoGroup;
        private readonly Editor editor;

        public CommandManager(Editor editor)
        {
            commands = new Dictionary<Type, CommandInfo>();
            commandsByKeys = new Dictionary<Keys, CommandInfo>();
            this.editor = editor;
        }

        public void BeginUndoAction()
        {
            counter++;
            undoGroup = true;
        }

        public void EndUndoAction()
        {
            undoGroup = false;
        }

        private void AddCommand(ICommand cmd, ActionExponent exp)
        {
            if (!undoGroup)
                counter++;

            editor.Buffer.UndoStack.Push(new CommandInfo { Id = counter, Command = cmd, Exponent = exp });
        }

        public void Undo()
        {
            if (editor.Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(editor.Buffer.UndoStack.Peek().Id, out count, out pos);

                SetCarets(count, pos);
                DoAftermath(exp);
                editor.Buffer.Edits--;
            }
        }

        private ActionExponent Undo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionExponent);
            pos = Pos.Empty;
            count = 0;

            while (editor.Buffer.UndoStack.Count > 0)
            {
                var cmd = editor.Buffer.UndoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = editor;
                    var p = cmd.Command.Undo();

                    if (pos.IsEmpty)
                        pos = p;

                    exp |= cmd.Exponent;
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

                SetCarets(count, pos);
                DoAftermath(exp);
                editor.Buffer.Edits++;
            }
        }

        private ActionExponent Redo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionExponent);
            pos = Pos.Empty;
            count = 0;

            while (editor.Buffer.RedoStack.Count > 0)
            {
                var cmd = editor.Buffer.RedoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = editor;
                    var p = cmd.Command.Redo();
                    pos = p;
                    exp |= cmd.Exponent;
                    AttachCaret(p);
                    editor.Buffer.UndoStack.Push(editor.Buffer.RedoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            return exp;
        }

        public void Register<T>(Keys keys = Keys.None) where T : ICommand, new()
        {
            var type = typeof(T);
            var attr = Attribute.GetCustomAttribute(type,
                typeof(CommandBehaviorAttribute)) as CommandBehaviorAttribute;
            var exp = attr != null ? attr.Exponent : ActionExponent.None;

            var ci = new CommandInfo
            {
                Command = new T(),
                Exponent = exp
            };

            commands.Add(type, ci);

            if (keys != Keys.None)
                commandsByKeys.Add(keys, ci);
        }

        public void Run<T>(CommandArgument arg) where T : ICommand
        {
            CommandInfo ci;

            if (commands.TryGetValue(typeof(T), out ci))
                Run(arg, ci);
        }

        public void Run(Keys keys, CommandArgument arg)
        {
            CommandInfo ci;

            if (commandsByKeys.TryGetValue(keys, out ci))
                Run(arg, ci);
        }

        private void Run(CommandArgument arg, CommandInfo ci)
        {
            var lines = editor.Lines;
            var exp = ci.Exponent;
            var single = editor.Buffer.Selections.Count == 1;
            var mainSel = editor.Buffer.Selections.Main;
            var qry = single ? null : editor.Buffer.Selections
                .OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var undo = (exp & ActionExponent.Undoable) == ActionExponent.Undoable;
            var restoreCaret = (exp & ActionExponent.RestoreCaret) == ActionExponent.RestoreCaret;
            var cmd = ci.Command;

            if (undo)
                BeginUndoAction();

            if ((exp & ActionExponent.SingleCursor) == ActionExponent.SingleCursor)
                editor.Buffer.Selections.Truncate();
            else if ((exp & ActionExponent.ClearSelections) == ActionExponent.ClearSelections)
            {
                if (single)
                    mainSel.Clear();
                else
                    foreach (var s in editor.Buffer.Selections)
                        s.Clear();
            }

            var lastSel = mainSel;

            if (single)
            {
                if (undo)
                {
                    cmd = cmd.Clone();
                    AddCommand(cmd, exp);
                }

                cmd.Context = editor;
                cmd.Execute(arg, mainSel);

                if (restoreCaret)
                    AttachCaret(mainSel.Caret);
            }
            else
                foreach (var sel in qry)
                {
                    if (undo)
                    {
                        cmd = cmd.Clone();
                        AddCommand(cmd, exp);
                    }

                    cmd.Context = editor;
                    cmd.Execute(arg, sel);

                    if (restoreCaret)
                        AttachCaret(sel.Caret);

                    lastSel = sel;
                }

            if (undo)
                EndUndoAction();

            if (restoreCaret)
                SetCarets(editor.Buffer.Selections.Count, lastSel.Caret);

            DoAftermath(exp);
            editor.Buffer.Edits++;
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

        private void DoAftermath(ActionExponent exp)
        {
            var scrolled = false;

            if ((exp & ActionExponent.Modify) == ActionExponent.Modify)
                editor.Scroll.InvalidateLines();

            if ((exp & ActionExponent.Scroll) == ActionExponent.Scroll)
                scrolled = editor.Scroll.UpdateVisibleRectangle();

            if ((exp & ActionExponent.Silent) != ActionExponent.Silent)
                editor.Redraw();

            if (((exp & ActionExponent.Scroll) == ActionExponent.Scroll && scrolled)
                || (exp & ActionExponent.Modify) == ActionExponent.Modify)
                editor.Styles.Restyle();
        }
    }
}
