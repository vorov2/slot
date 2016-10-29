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
    public sealed class DeleteBackCommand : Command //Tested
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Pos undoPos;
        private Selection redoSel;

        public override bool Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();
            var res = false;

            if (!sel.IsEmpty)
            {
                @string = DeleteRangeCommand.DeleteRange(Context, sel);
                res = true;
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
                    res = true;
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
                    res = true;
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
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);
            else if (@char == Character.NewLine)
            {
                pos = new Pos(undoPos.Line, undoPos.Col);
                var txt = DeleteRangeCommand.DeleteRange(Context, new Selection(pos,
                    new Pos(pos.Line, Document.Lines[pos.Line].Length)));
                var ipos = InsertNewLineCommand.InsertNewLine(Document, pos);
                Document.Lines[ipos.Line].Append(txt);
                pos = ipos;
            }
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
                pos = new Pos(undoPos.Line, undoPos.Col + 1);
            }

            return pos;
        }
    }
}
