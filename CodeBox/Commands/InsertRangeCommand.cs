using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public class InsertRangeCommand : Command, IModifyContent
    {
        private Selection undo;
        protected Selection redoSel;
        private IEnumerable<Character> @string;
        private IEnumerable<Character> @redoString;

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(Context, sel);

            @redoString = arg.String.MakeCharacters();
            var pos = InsertRange(Document, sel.Start, @redoString);
            undo = new Selection(sel.Start, pos);
            sel.Clear(pos);
            return Change;
        }

        public override ActionResults Redo(out Pos pos)
        {
            @string = null;
            var sel = redoSel;
            var arg = new CommandArgument(redoString.MakeString(Context.Buffer.Eol));
            @redoString = null;
            Execute(arg, sel);
            pos = sel.Caret;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            DeleteRangeCommand.DeleteRange(Context, undo);
            pos = undo.Caret;

            if (@string != null)
                pos = InsertRange(Document, undo.End, @string);

            return Change;
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

        public override ICommand Clone()
        {
            return new InsertRangeCommand();
        }
    }
}
