using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.delete")]
    public class DeleteCommand : EditorCommand
    {
        private IEnumerable<Character> deleteString;
        private Character deleteChar;
        private Selection redoSel;
        protected Pos undoPos;

        protected override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var res = Clean;

            if (!sel.IsEmpty)
            {
                res = Change;
                deleteString = DeleteRangeCommand.DeleteRange(View, sel);
            }
            else
            {
                var lines = Document.Lines;
                var caret = sel.Caret;
                var ln = lines[caret.Line];

                if (caret.Col < ln.Length)
                {
                    deleteChar = ln.CharacterAt(caret.Col);
                    ln.RemoveAt(caret.Col);
                    res = Change;
                }
                else if (caret.Line < lines.Count - 1)
                {
                    var nl = lines[caret.Line + 1];
                    deleteChar = Character.NewLine;
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
                InsertNewLineCommand.InsertNewLine(Document, undoPos);
            else
            {
                var ln = Document.Lines[undoPos.Line];
                ln.Insert(undoPos.Col, deleteChar);
            }

            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new DeleteCommand();
        }

        public override bool ModifyContent => true;

        public override bool SupportLimitedMode => true;
    }
}
