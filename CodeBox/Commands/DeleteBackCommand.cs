using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.RestoreCaret | ActionExponent.Scroll | ActionExponent.Undoable)]
    internal sealed class DeleteBackCommand : Command //Tested
    {
        private IEnumerable<Character> @string;
        private Character @char;
        private Pos undoPos;
        private Selection redoSel;

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

                if (caret.Col > 0)
                {
                    @char = ln.CharacterAt(caret.Col - 1);
                    ln.RemoveAt(caret.Col - 1);
                    sel.Clear(new Pos(caret.Line, caret.Col - 1));
                }
                else if (caret.Line > 0)
                {
                    ln = lines[caret.Line - 1];
                    var col = ln.Length;
                    @char = Character.NewLine;
                    var txt = lines[caret.Line];
                    lines.RemoveAt(caret.Line);
                    ln.Append(txt);
                    sel.Clear(new Pos(ln.Index, col));
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
            {
                pos = new Pos(undoPos.Line, undoPos.Col);
                var txt = DeleteRangeCommand.DeleteRange(context, new Selection(pos, new Pos(pos.Line, context.Document.Lines[pos.Line].Length)));
                var ipos = InsertNewLineCommand.InsertNewLine(context.Document, pos);
                context.Document.Lines[ipos.Line].Append(txt);
                pos = ipos;
            }
            else
            {
                var ln = context.Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, @char);
                pos = new Pos(undoPos.Line, undoPos.Col + 1);
            }

            return pos;
        }
    }
}
