using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.redo")]
    public sealed class RedoCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            Redo();
            return Pure;
        }

        private void Redo()
        {
            if (Buffer.RedoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Redo(Buffer.RedoStack.Peek().Id, out count, out pos);
                SetEditLines();
                DoAftermath(exp, count, pos);
            }
        }

        private ActionResults Redo(int id, out int count, out Pos pos)
        {
            var exp = default(ActionResults);
            pos = Pos.Empty;
            count = 0;

            while (Buffer.RedoStack.Count > 0)
            {
                var cmd = Buffer.RedoStack.Peek();

                if (cmd.Id == id)
                {
                    cmd.Command.Context = Context;
                    Pos p;
                    var e = cmd.Command.Redo(out p);
                    pos = p;
                    exp |= e;

                    if (e.Has(RestoreCaret))
                        AttachCaret(p);

                    Buffer.UndoStack.Push(Buffer.RedoStack.Pop());
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
