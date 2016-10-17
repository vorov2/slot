using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    internal sealed class DeleteCommand : Command //Tested
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Selection redoSel;
        private Pos undoPos;

        public override void Execute(EditorContext context, Selection sel)
        {
            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(context, sel);
            else
            {
                var lines = context.Document.Lines;
                var caret = sel.Caret;
                var ln = lines[caret.Line];

                if (caret.Col < ln.Length)
                {
                    @char = ln.CharacterAt(caret.Col);
                    ln.RemoveAt(caret.Col);
                }
                else if (caret.Line < lines.Count)
                {
                    var nl = lines[caret.Line + 1];
                    @char = Character.NewLine;
                    lines.Remove(nl);
                    ln.Append(nl);
                }
            }

            undoPos = sel.Caret;
        }

        public override Pos Redo(EditorContext context)
        {
            @string = null;
            @char = Character.Empty;
            Execute(context, redoSel);
            return undoPos;
        }

        public override Pos Undo(EditorContext context)
        {
            var pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(context.Document, undoPos, @string);
            else if (@char == Character.NewLine)
                InsertNewLineCommand.InsertNewLine(context.Document, undoPos);
            else
            {
                var ln = context.Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
            }

            return pos;
        }
    }
}
