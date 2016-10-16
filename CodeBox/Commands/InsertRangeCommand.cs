using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.RestoreCaret | ActionExponent.Undoable | ActionExponent.Scroll)]
    internal class InsertRangeCommand : Command //Tested
    {
        private Selection undo;
        protected Selection redoSel;
        private IEnumerable<Character> @string;
        private IEnumerable<Character> @redoString;

        public override void Execute(EditorContext context, Selection sel)
        {
            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(context, sel);

            @redoString = context.String.MakeCharacters();
            var pos = InsertRange(context.Document, sel.Start, @redoString);
            undo = new Selection(sel.Start, pos);
            sel.Clear(pos);
        }

        public override Pos Redo(EditorContext context)
        {
            @string = null;
            var sel = redoSel;
            context.String = redoString.MakeString(context.Eol);
            @redoString = null;
            Execute(context, sel);
            return sel.Caret;
        }

        public override Pos Undo(EditorContext context)
        {
            DeleteRangeCommand.DeleteRange(context, undo);
            var pos = Pos.Empty;

            if (@string != null)
                pos = InsertRange(context.Document, undo.End, @string);

            if (pos.IsEmpty)
                pos = undo.Caret;

            return pos;
        }

        internal static Pos InsertRange(Document doc, Pos pos, IEnumerable<Character> textRange)
        {
            if (textRange != null)
            {
                var retPos = Pos.Empty;
                var arr = textRange.MakeLines();
                var startLine = doc.Lines[pos.Line];
                var len = arr.Count();

                if (len == 1)
                {
                    startLine.Insert(pos.Col, textRange);
                    retPos = new Pos(startLine.Index, pos.Col + textRange.Count());
                }
                else
                {
                    var portion = startLine.GetRange(pos.Col, startLine.Length - pos.Col);
                    var i = 0;

                    foreach (var ln in arr)
                    {
                        var first = i == 0;
                        var last = i == len - 1;

                        if (first)
                        {
                            startLine.RemoveRange(pos.Col, startLine.Length - pos.Col);
                            startLine.Insert(pos.Col, ln);
                            retPos = new Pos(startLine.Index, startLine.Length);
                        }
                        else if (last)
                        {
                            var line = new Line(ln, ++doc.LineSequence);
                            line.Append(portion);
                            doc.Lines.Insert(pos.Line + i, line);
                            retPos = new Pos(pos.Line + i, ln.Count());
                        }
                        else
                        {
                            var line = new Line(ln, ++doc.LineSequence);
                            doc.Lines.Insert(pos.Line + i, line);
                            retPos = new Pos(pos.Line + i, line.Length);
                        }

                        i++;
                    }
                }

                return retPos;
            }

            return Pos.Empty;
        }
    }
}
