using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.undo", "euu")]
    public sealed class UndoCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            Undo();
            return Pure | KeepRedo;
        }

        private void Undo()
        {
            if (Buffer.UndoStack.Count > 0)
            {
                Pos pos;
                int count;
                var exp = Undo(Buffer.UndoStack.Peek().Id, out count, out pos);
                SetEditLines();
                DoAftermath(exp | KeepRedo, count, pos, -1);
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
                    cmd.Command.View = View;
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
