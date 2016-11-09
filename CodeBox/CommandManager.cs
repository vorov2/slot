using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.Commands;
using CodeBox.Styling;

namespace CodeBox
{
    public sealed class CommandManager
    {
        private readonly Dictionary<Type, CommandInfo> commands;
        private readonly Dictionary<CommandKeys, List<CommandInfo>> commandsByKeys;
        private int counter;
        private bool undoGroup;
        private readonly Editor editor;

        internal CommandManager(Editor editor)
        {
            commands = new Dictionary<Type, CommandInfo>();
            commandsByKeys = new Dictionary<CommandKeys, List<CommandInfo>>();
            this.editor = editor;
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            Register<AutocompleteCommand>();
            Register<SelectLineCommand>();
            Register<ToggleFoldingCommand>();
            Register<FollowLinkCommand>();
            Register<DeleteWordBackCommand>();
            Register<DeleteWordCommand>();
            Register<ScrollLineUpCommand>();
            Register<ScrollLineDownCommand>();
            Register<RedoCommand>();
            Register<UndoCommand>();
            Register<SetCaretCommand>();
            Register<AddCaretCommand>();
            Register<NormalSelectCommand>();
            Register<BlockSelectCommand>();
            Register<InsertCharCommand>();
            Register<SelectWordCommand>();
            Register<CutCommand>();
            Register<CopyCommand>();
            Register<PasteCommand>();
            Register<SelectAllCommand>();
            Register<ShiftTabCommand>();
            Register<TabCommand>();
            Register<ClearSelectionCommand>();
            Register<LeftCommand>();
            Register<RightCommand>();
            Register<UpCommand>();
            Register<DownCommand>();
            Register<HomeCommand>();
            Register<EndCommand>();
            Register<InsertNewLineCommand>();
            Register<DeleteBackCommand>();
            Register<DeleteCommand>();
            Register<PageDownCommand>();
            Register<PageUpCommand>();
            Register<ExtendLeftCommand>();
            Register<ExtendRightCommand>();
            Register<ExtendUpCommand>();
            Register<ExtendDownCommand>();
            Register<ExtendEndCommand>();
            Register<ExtendHomeCommand>();
            Register<WordLeftCommand>();
            Register<WordRightCommand>();
            Register<ExtendWordRightCommandCommand>();
            Register<ExtendWordLeftCommandCommand>();
            Register<ExtendPageDownCommand>();
            Register<ExtendPageUpCommand>();
            Register<DocumentHomeCommand>();
            Register<DocumentEndCommand>();
            Register<ExtendDocumentHomeCommand>();
            Register<ExtendDocumentEndCommand>();
            Register<OvertypeCommand>();
            Register<InsertRangeCommand>();
            Register<DeleteRangeCommand>();
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

        private void AddCommand(ICommand cmd, ActionExponent exp) =>
            editor.Buffer.UndoStack.Push(new CommandInfo { Id = counter, Command = cmd, Exponent = exp });

        public void Undo()
        {
            if (editor.Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(editor.Buffer.UndoStack.Peek().Id, out count, out pos);

                DoAftermath(exp, ActionResults.Change);
                SetCarets(count, pos);
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
                    cmd.Command.Context = editor.Context;
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

                DoAftermath(exp, ActionResults.Change);
                SetCarets(count, pos);
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
                    cmd.Command.Context = editor.Context;
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

        public void Register<T>() where T : ICommand, new()
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
        }

        public bool Bind<T>(Keys keys) where T : ICommand =>
            Bind<T>(MouseEvents.None, keys);

        public bool Bind<T>(MouseEvents mouse, Keys keys) where T : ICommand
        {
            var type = typeof(T);
            CommandInfo ci;

            if (!commands.TryGetValue(type, out ci))
                return false;

            var ck = new CommandKeys(mouse, keys);
            List<CommandInfo> cilist;

            if (!commandsByKeys.TryGetValue(ck, out cilist))
            {
                cilist = new List<CommandInfo>();
                commandsByKeys.Add(ck, cilist);
            }

            cilist.Add(ci);
            return true;
        }

        public void Run<T>(CommandArgument arg) where T : ICommand
        {
            CommandInfo ci;

            if (commands.TryGetValue(typeof(T), out ci))
                Run(arg, ci);
        }

        public void Run(Keys keys, CommandArgument arg) => Run(MouseEvents.None, keys, arg);

        public void Run(MouseEvents mouse, Keys keys, CommandArgument arg)
        {
            List<CommandInfo> seq;

            if (commandsByKeys.TryGetValue(new CommandKeys(mouse, keys), out seq))
                foreach (var ci in seq)
                    Run(arg, ci);
        }

        private void Run(CommandArgument arg, CommandInfo ci)
        {
            FirstEditLine = -1;
            LastEditLine = -1;
            var lines = editor.Lines;
            var exp = ci.Exponent;
            var single = editor.Buffer.Selections.Count == 1
                || (exp & ActionExponent.SingleRun) == ActionExponent.SingleRun;
            var mainSel = editor.Buffer.Selections.Main;
            var qry = single ? null : editor.Buffer.Selections
                .OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var undo = (exp & ActionExponent.Undoable) == ActionExponent.Undoable;
            var restoreCaret = (exp & ActionExponent.RestoreCaret) == ActionExponent.RestoreCaret;
            var cmd = ci.Command;
            var thisUndo = false;
            var exec = ActionResults.None;

            if (undo)
                thisUndo = BeginUndoAction();

            if ((exp & ActionExponent.ClearSelections) == ActionExponent.ClearSelections)
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

                FirstEditLine = mainSel.GetFirstLine();
                LastEditLine = mainSel.GetLastLine();
                cmd.Context = editor.Context;

                if ((exec = cmd.Execute(arg, mainSel)) == ActionResults.None && undo)
                    editor.Buffer.UndoStack.Pop();

                if (restoreCaret)
                    AttachCaret(mainSel.Caret);
            }
            else
            {
                foreach (var sel in qry)
                {
                    if (undo)
                    {
                        cmd = cmd.Clone();
                        AddCommand(cmd, exp);
                    }

                    FirstEditLine = sel.GetFirstLine();
                    LastEditLine = sel.GetLastLine();

                    cmd.Context = editor.Context;
                    var e = cmd.Execute(arg, sel);

                    if (e != ActionResults.None)
                        exec |= e;

                    if (e == ActionResults.None && undo)
                        editor.Buffer.UndoStack.Pop();

                    if (restoreCaret)
                        AttachCaret(sel.Caret);

                    lastSel = sel;
                }
            }

            if (thisUndo)
                EndUndoAction();

            if (exec != ActionResults.None)
                DoAftermath(exp, exec);

            if (restoreCaret)
                SetCarets(editor.Buffer.Selections.Count, lastSel.Caret);

            if ((exp & ActionExponent.IdleCaret) != ActionExponent.IdleCaret)
                editor.MatchBraket.Match();

            if (exec.Has(ActionResults.AutocompleteKeep) && editor.Autocomplete.WindowShown)
                editor.Autocomplete.UpdateAutocomplete();
            else if (!exec.Has(ActionResults.AutocompleteShow))
                editor.Autocomplete.HideAutocomplete();
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

        private void DoAftermath(ActionExponent exp, ActionResults exec)
        {
            if ((exp & ActionExponent.Modify) == ActionExponent.Modify
                || (exp & ActionExponent.Invalidate) ==  ActionExponent.Invalidate)
                editor.Scroll.InvalidateLines(
                    exec.Has(ActionResults.AtomicChange) ? ScrollingManager.InvalidateFlags.Atomic
                    : ScrollingManager.InvalidateFlags.None
                    );

            if ((exp & ActionExponent.Scroll) == ActionExponent.Scroll)
            {
                editor.Scroll.SuppressOnScroll = true;
                editor.Scroll.UpdateVisibleRectangle();
                editor.Scroll.SuppressOnScroll = false;
            }

            if ((exp & ActionExponent.Scroll) == ActionExponent.Scroll
                || (exp & ActionExponent.Modify) == ActionExponent.Modify)
                editor.Styles.Restyle();

            if ((exp & ActionExponent.Silent) != ActionExponent.Silent)
                editor.Redraw();

            if ((exp & ActionExponent.Modify) == ActionExponent.Modify)
            {
                editor.Folding.RebuildFolding();
                editor.Buffer.Edits++;
            }

            if ((exp & ActionExponent.LeaveEditor) == ActionExponent.LeaveEditor)
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
