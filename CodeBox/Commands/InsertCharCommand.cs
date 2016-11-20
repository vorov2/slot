using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.Affinity;

namespace CodeBox.Commands
{
    public class InsertCharCommand : EditorCommand
    {
        private Character deleteChar;
        private Character insertChar;
        private IEnumerable<Character> insertString;
        private Pos undoPos;
        private Selection redoSel;

        public InsertCharCommand(Character insertChar)
        {
            this.insertChar = insertChar;
        }

        protected override ActionResults Execute(Selection sel)
        {
            var line = Document.Lines[sel.Caret.Line];
            undoPos = sel.Start;
            redoSel = sel.Clone();
            var res = Change;

            if (!sel.IsEmpty)
                insertString = DeleteRangeCommand.DeleteRange(View, sel);
            else if (Buffer.Overtype && sel.Caret.Col < line.Length)
            {
                res |= AtomicChange;
                deleteChar = line[sel.Caret.Col];
                line.RemoveAt(sel.Caret.Col);
            }
            else
            {
                var app = !Buffer.Overtype && CanShowAutocomplete(sel, insertChar.Char) ? AutocompleteShow : AutocompleteKeep;
                res |= AtomicChange | app;
            }

            Document.Lines[sel.Caret.Line].Insert(sel.Caret.Col, insertChar);
            sel.Clear(new Pos(sel.Caret.Line, sel.Caret.Col + 1));
            return res;
        }

        //TODO: check performance
        private bool CanShowAutocomplete(Selection sel, char c)
        {
            var aff = View.AffinityManager.GetAffinity(sel.Caret);
            var sym = aff.GetAutocompleteSymbols(View);
            return (sym != null ? sym.IndexOf(c) != -1 : false) || char.IsLetter(c);
        }

        public override ActionResults Redo(out Pos pos)
        {
            insertString = null;
            deleteChar = Character.Empty;
            Execute(redoSel);
            pos = new Pos(redoSel.Start.Line, redoSel.Start.Col + 1);
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            var lines = Document.Lines;
            lines[undoPos.Line].RemoveAt(undoPos.Col);
            pos = Pos.Empty;

            if (insertString != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, insertString);

            if (!deleteChar.IsEmpty)
                lines[undoPos.Line].Insert(undoPos.Col, deleteChar);

            if (pos.IsEmpty)
                pos = undoPos;

            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new InsertCharCommand(insertChar);
        }

        public override bool ModifyContent => true;
    }
}
