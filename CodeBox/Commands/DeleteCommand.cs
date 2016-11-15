using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public class DeleteCommand : Command, IModifyContent
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Selection redoSel;
        private Pos undoPos;

        public override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var res = Pure;

            if (!sel.IsEmpty)
            {
                res = Change;
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
                    res = Change;
                }
                else if (caret.Line < lines.Count - 1)
                {
                    var nl = lines[caret.Line + 1];
                    @char = Character.NewLine;
                    lines.Remove(nl);
                    ln.Append(nl);
                    res = Change;
                }
            }

            undoPos = sel.Caret;
            return res;
        }

        public override ActionResults Redo(out Pos pos)
        {
            @string = null;
            @char = Character.Empty;
            Execute(redoSel);
            pos = undoPos;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);
            else if (@char == Character.NewLine)
                InsertNewLineCommand.InsertNewLine(Document, undoPos);
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
            }

            return Change;
        }

        public override ICommand Clone()
        {
            return new DeleteCommand();
        }
    }
}
