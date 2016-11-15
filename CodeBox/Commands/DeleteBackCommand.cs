using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public class DeleteBackCommand : Command, IModifyContent
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Pos undoPos;
        private Selection redoSel;

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

                if (caret.Col > 0)
                {
                    @char = ln.CharacterAt(caret.Col - 1);
                    ln.RemoveAt(caret.Col - 1);
                    sel.Clear(new Pos(caret.Line, caret.Col - 1));
                    res = Change | AutocompleteKeep;
                }
                else if (caret.Line > 0)
                {
                    ln = lines[caret.Line - 1];
                    var col = ln.Length;
                    @char = Character.NewLine;
                    var txt = lines[caret.Line];
                    lines.RemoveAt(caret.Line);
                    ln.Append(txt);
                    sel.Clear(new Pos(caret.Line - 1, col));
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
            {
                pos = new Pos(undoPos.Line, undoPos.Col);
                var txt = DeleteRangeCommand.DeleteRange(Context, new Selection(pos,
                    new Pos(pos.Line, Document.Lines[pos.Line].Length)));
                var ipos = InsertNewLineCommand.InsertNewLine(Document, pos);

                if (txt != null)
                    Document.Lines[ipos.Line].Append(txt);

                pos = ipos;
            }
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
                pos = new Pos(undoPos.Line, undoPos.Col + 1);
            }

            return Change;
        }

        public override ICommand Clone()
        {
            return new DeleteBackCommand();
        }
    }
}
