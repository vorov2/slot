using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.insertrange", "eeir")]
    public class InsertRangeCommand : EditorCommand
    {
        private Selection undo;
        protected Selection redoSel;
        private IEnumerable<Character> deleteString;
        protected IEnumerable<Character> insertString;

        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            insertString = insertString ?? arg as IEnumerable<Character>;

            if (insertString == null)
                return Pure;

            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                deleteString = DeleteRangeCommand.DeleteRange(View, sel);

            var pos = InsertRange(Document, sel.Start, insertString);
            undo = new Selection(sel.Start, pos);
            sel.Clear(pos);
            return Change;
        }

        public override ActionResults Redo(out Pos pos)
        {
            deleteString = null;
            var sel = redoSel;
            Execute(sel, insertString);
            pos = sel.Caret;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            DeleteRangeCommand.DeleteRange(View, undo);
            pos = undo.Caret;

            if (deleteString != null)
                pos = InsertRange(Document, undo.End, deleteString);

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

        internal override EditorCommand Clone()
        {
            return new InsertRangeCommand();
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => true;
    }
}
