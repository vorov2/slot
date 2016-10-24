using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Undoable | Scroll)]
    internal class InsertRangeCommand : Command //Tested
    {
        private Selection undo;
        protected Selection redoSel;
        private IEnumerable<Character> @string;
        private IEnumerable<Character> @redoString;

        public override void Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(Context, sel);

            @redoString = arg.String.MakeCharacters();
            var pos = InsertRange(Document, sel.Start, @redoString);
            undo = new Selection(sel.Start, pos);
            sel.Clear(pos);
        }

        public override Pos Redo()
        {
            @string = null;
            var sel = redoSel;
            var arg = new CommandArgument('\0', redoString.MakeString(Context.Buffer.Eol));
            @redoString = null;
            Execute(arg, sel);
            return sel.Caret;
        }

        public override Pos Undo()
        {
            DeleteRangeCommand.DeleteRange(Context, undo);
            var pos = Pos.Empty;

            if (@string != null)
                pos = InsertRange(Document, undo.End, @string);

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
                    retPos = new Pos(pos.Line, pos.Col + textRange.Count());
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
                            retPos = new Pos(pos.Line, startLine.Length);
                        }
                        else if (last)
                        {
                            var line = doc.NewLine(ln);
                            line.Append(portion);
                            doc.Lines.Insert(pos.Line + i, line);
                            retPos = new Pos(pos.Line + i, ln.Count());
                        }
                        else
                        {
                            var line = doc.NewLine(ln);
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
