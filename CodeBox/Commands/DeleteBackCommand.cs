using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.deleteback")]
    public class DeleteBackCommand : EditorCommand
    {
        private IEnumerable<Character> deleteString;
        private Character deleteChar;
        private Pos undoPos;
        private Selection redoSel;

        public override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var res = Clean;
            var lines = Document.Lines;
            var caret = sel.Caret;
            var ln = lines[caret.Line];

            if (!sel.IsEmpty)
            {
                res = Change;
                deleteString = DeleteRangeCommand.DeleteRange(Context, sel);
            }
            else if (!Context.UseTabs && caret.Col >= Context.IndentSize
                && ln.CharAt(caret.Col - 1) == ' ' && ln.CharAt(caret.Col - 2) == ' '
                && ln.CharAt(caret.Col - 3) == ' ' && ln.CharAt(caret.Col - 4) == ' ')
            {
                res = Change;
                deleteString = DeleteRangeCommand.DeleteRange(Context, new Selection(caret, new Pos(caret.Line, caret.Col - 4)));
            }
            else
            {
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

        public override ActionResults Redo(out Pos pos)
        {
            deleteString = null;
            deleteChar = Character.Empty;
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
    }
}
