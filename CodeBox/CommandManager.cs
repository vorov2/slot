using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.Commands;
using CodeBox.ObjectModel;

namespace CodeBox
{
    internal sealed class CommandManager
    {
        private readonly Dictionary<Guid, CommandInfo> commands;
        private readonly Dictionary<Keys, CommandInfo> commandsByKeys;
        private readonly Stack<CommandInfo> undoStack;
        private readonly Stack<CommandInfo> redoStack;
        private int counter;
        private bool undoGroup;
        private readonly Editor editor;

        class CommandInfo
        {
            internal int Id;
            internal ICommand Command;
            internal ActionExponent Exponent;
        }

        public CommandManager(Editor editor)
        {
            commands = new Dictionary<Guid, CommandInfo>();
            commandsByKeys = new Dictionary<Keys, CommandInfo>();
            undoStack = new Stack<CommandInfo>();
            redoStack = new Stack<CommandInfo>();
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

            undoStack.Push(new CommandInfo { Id = counter, Command = cmd, Exponent = exp });
        }

        public void Undo(EditorContext ctx)
        {
            if (undoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(ctx, undoStack.Peek().Id, out count, out pos);

                SetCarets(ctx, count, pos);
                DoAftermath(exp);
            }
        }

        private ActionExponent Undo(EditorContext ctx, int id, out int count, out Pos pos)
        {
            var exp = default(ActionExponent);
            pos = Pos.Empty;
            count = 0;

            while (undoStack.Count > 0)
            {
                var cmd = undoStack.Peek();

                if (cmd.Id == id)
                {
                    var p = cmd.Command.Undo(ctx);

                    if (pos.IsEmpty)
                        pos = p;

                    exp |= cmd.Exponent;
                    AttachCaret(ctx, p);
                    redoStack.Push(undoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            return exp;
        }

        public void Redo(EditorContext ctx)
        {
            if (redoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Redo(ctx, redoStack.Peek().Id, out count, out pos);

                SetCarets(ctx, count, pos);
                DoAftermath(exp);
            }
        }

        private ActionExponent Redo(EditorContext ctx, int id, out int count, out Pos pos)
        {
            var exp = default(ActionExponent);
            pos = Pos.Empty;
            count = 0;

            while (redoStack.Count > 0)
            {
                var cmd = redoStack.Peek();

                if (cmd.Id == id)
                {
                    var p = cmd.Command.Redo(ctx);
                    pos = p;
                    exp |= cmd.Exponent;
                    AttachCaret(ctx, p);
                    undoStack.Push(redoStack.Pop());
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
            var attr = Attribute.GetCustomAttribute(type, typeof(CommandBehaviorAttribute)) as CommandBehaviorAttribute;
            var exp = attr != null ? attr.Exponent : ActionExponent.None;

            var ci = new CommandInfo
            {
                Command = new T(),
                Exponent = exp
            };

            commands.Add(type.GUID, ci);

            if (keys != Keys.None)
                commandsByKeys.Add(keys, ci);
        }

        public void Run<T>(EditorContext ctx) where T : ICommand
        {
            CommandInfo ci;

            if (commands.TryGetValue(typeof(T).GUID, out ci))
                Run(ctx, ci);
        }

        public void Run(Keys keys, EditorContext ctx)
        {
            CommandInfo ci;

            if (commandsByKeys.TryGetValue(keys, out ci))
                Run(ctx, ci);
        }

        private void Run(EditorContext ctx, CommandInfo ci)
        {
            var lines = ctx.Document.Lines;
            var exp = ci.Exponent;
            var single = ctx.Document.Selections.TotalCount == 1;
            var mainSel = ctx.Document.Selections.Main;
            var qry = single ? null : editor.Document.Selections
                .OrderByDescending(s => s.End > s.Start ? s.Start : s.End);
            var undo = (exp & ActionExponent.Undoable) == ActionExponent.Undoable;
            var restoreCaret = (exp & ActionExponent.RestoreCaret) == ActionExponent.RestoreCaret;
            var cmd = ci.Command;

            if (undo)
                BeginUndoAction();

            if ((exp & ActionExponent.SingleCursor) == ActionExponent.SingleCursor)
                ctx.Document.Selections.Clear();
            else if ((exp & ActionExponent.ClearSelections) == ActionExponent.ClearSelections)
            {
                if (single)
                    mainSel.Clear();
                else
                    foreach (var s in ctx.Document.Selections)
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

                cmd.Execute(ctx, mainSel);

                if (restoreCaret)
                    AttachCaret(ctx, mainSel.Caret);
            }
            else
                foreach (var sel in qry)
                {
                    if (undo)
                    {
                        cmd = cmd.Clone();
                        AddCommand(cmd, exp);
                    }
                
                    cmd.Execute(ctx, sel);

                    if (restoreCaret)
                        AttachCaret(ctx, sel.Caret);

                    lastSel = sel;
                }

            if (undo)
                EndUndoAction();

            if (restoreCaret)
                SetCarets(ctx, ctx.Document.Selections.TotalCount, lastSel.Caret);

            DoAftermath(exp);
        }

        private void AttachCaret(EditorContext ctx, Pos pos)
        {
            var line = ctx.Document.Lines[pos.Line];

            if (line.Length > pos.Col)
            {
                var ch = line.CharacterAt(pos.Col).WithCaret();
                line[pos.Col] = ch;
            }
            else
                line.TrailingCaret = true;
        }

        private void SetCarets(EditorContext ctx, int count, Pos pos)
        {
            var sels = ctx.Document.Selections;
            sels.ForceClear();

            for (var i = pos.Line; i < ctx.Document.Lines.Count; i++)
            {
                var line = ctx.Document.Lines[i];

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
                editor.InvalidateLines();

            if ((exp & ActionExponent.Scroll) == ActionExponent.Scroll)
                scrolled = editor.UpdateVisibleRectangle();

            if ((exp & ActionExponent.Silent) != ActionExponent.Silent)
                editor.Redraw();

            if (((exp & ActionExponent.Scroll) == ActionExponent.Scroll && scrolled)
                || (exp & ActionExponent.Modify) == ActionExponent.Modify)
                editor.Restyle();
        }
    }
}
