using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.deleteback", "eedb")]
    public class DeleteBackCommand : EditorCommand
    {
        private IEnumerable<Character> deleteString;
        private Character deleteChar;
        private Pos undoPos;
        protected Selection redoSel;
        private int unindent;

        protected override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var res = Clean;
            var lines = Document.Lines;
            var caret = sel.Caret;
            var ln = lines[caret.Line];
            var indentSize = Line.GetIndentationSize(ln.GetTetras(caret.Col, View.IndentSize), View.IndentSize);
            unindent = indentSize == View.IndentSize ? indentSize : View.IndentSize - indentSize;

            if (!sel.IsEmpty)
            {
                unindent = 0;
                res = Change;
                deleteString = DeleteRangeCommand.DeleteRange(View, sel);
            }
            else if (!View.UseTabs && (unindent = GetMaximum(ln, caret.Col, unindent)) > 1 && caret.Col >= unindent)
            {
                res = Change;
                var np = new Pos(caret.Line, caret.Col - unindent);
                deleteString = DeleteRangeCommand.DeleteRange(View, new Selection(caret, np));
                sel.Clear(np);
            }
            else
            {
                unindent = 0;

                if (caret.Col > 0)
                {
                    deleteChar = ln.CharacterAt(caret.Col - 1);
                    ln.RemoveAt(caret.Col - 1);
                    sel.Clear(new Pos(caret.Line, caret.Col - 1));
                    res = Change | AutocompleteKeep;
                }
                else if (caret.Line > 0)
                {
                    ln = lines[caret.Line - 1];
                    var col = ln.Length;
                    deleteChar = Character.NewLine;
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

        private int GetMaximum(Line ln, int col, int num)
        {
            var c = 0;

            for (var i = col - 1; i > -1; i--)
                if (ln.CharAt(i) != ' ')
                    return c > num ? num : c;
                else
                    c++;

            return c > num ? num : c;
        }

        public override ActionResults Redo(out Pos pos)
        {
            deleteChar = Character.Empty;
            deleteString = null;

            if (unindent != 0)
            {
                var caret = redoSel.Caret;
                var np = new Pos(caret.Line, caret.Col - unindent);
                DeleteRangeCommand.DeleteRange(View, new Selection(caret, np));
            }
            else
                Execute(redoSel);

            pos = undoPos;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            pos = undoPos;

            if (deleteString != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, deleteString);
            else if (deleteChar == Character.NewLine)
            {
                pos = new Pos(undoPos.Line, undoPos.Col);
                var txt = DeleteRangeCommand.DeleteRange(View, new Selection(pos,
                    new Pos(pos.Line, Document.Lines[pos.Line].Length)));
                var ipos = InsertNewLineCommand.InsertNewLine(Document, pos);

                if (txt != null)
                    Document.Lines[ipos.Line].Append(txt);

                pos = ipos;
            }
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, deleteChar);
                pos = new Pos(undoPos.Line, undoPos.Col + 1);
            }

            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new DeleteBackCommand();
        }

        public override bool ModifyContent => true;

        public override bool SupportLimitedMode => true;
    }
}
