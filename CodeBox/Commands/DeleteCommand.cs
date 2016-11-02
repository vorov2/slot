using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public class DeleteCommand : Command
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Selection redoSel;
        private Pos undoPos;

        public override ActionResult Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();
            var res = ActionResult.None;

            if (!sel.IsEmpty)
            {
                res = ActionResult.Mixed;
                @string = DeleteRangeCommand.DeleteRange(Context, sel);
            }
            else
            {
                var lines = Document.Lines;
                var caret = sel.Caret;
                var ln = lines[caret.Line];

                if (caret.Col < ln.Length)
                {
                    @char = ln.CharacterAt(caret.Col);
                    ln.RemoveAt(caret.Col);
                    res = ActionResult.Forward;
                }
                else if (caret.Line < lines.Count - 1)
                {
                    var nl = lines[caret.Line + 1];
                    @char = Character.NewLine;
                    lines.Remove(nl);
                    ln.Append(nl);
                    res = ActionResult.Forward;
                }
            }

            undoPos = sel.Caret;
            return res;
        }

        public override Pos Redo()
        {
            @string = null;
            @char = Character.Empty;
            Execute(default(CommandArgument), redoSel);
            return undoPos;
        }

        public override Pos Undo()
        {
            var pos = undoPos;

            if (@string != null)
                InsertRangeCommand.InsertRange(Document, undoPos, @string);
            else if (@char == Character.NewLine)
                InsertNewLineCommand.InsertNewLine(Document, undoPos);
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
            }

            return pos;
        }
    }
}
