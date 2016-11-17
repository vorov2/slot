using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.undo")]
    public sealed class UndoCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            Undo();
            return Pure;
        }

        private void Undo()
        {
            if (Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(Buffer.UndoStack.Peek().Id, out count, out pos);
                SetEditLines();
                DoAftermath(exp, count, pos);
            }
        }

        private ActionResults Undo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;

            while (Buffer.UndoStack.Count > 0)
            {
                var cmd = Buffer.UndoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = Context;
                    Pos p;
                    var e = cmd.Command.Undo(out p);

                    if (pos.IsEmpty || p < pos)
                        pos = p;

                    exp |= e;

                    if (e.Has(RestoreCaret))
                        AttachCaret(p);

                    Buffer.RedoStack.Push(Buffer.UndoStack.Pop());
                    count++;
                }
                else
                    break;
            }

            return exp;
        }

        public override bool SingleRun => true;
    }
}
