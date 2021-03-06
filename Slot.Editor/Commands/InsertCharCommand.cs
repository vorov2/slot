﻿using System;
using System.Collections.Generic;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using Slot.Editor.Affinity;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public class InsertCharCommand : EditorCommand
    {
        public const string Name = "editor.insertChar";

        private Character deleteChar;
        private Character insertChar;
        private IEnumerable<Character> insertString;
        private Pos undoPos;
        private Selection redoSel;

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            insertChar = new Character(Ed.InputChar);

            var line = Document.Lines[sel.Caret.Line];
            undoPos = sel.Start;
            redoSel = sel.Clone();
            var res = Change;

            if (!sel.IsEmpty)
                insertString = DeleteRangeCommand.DeleteRange(Ed, sel);
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
            var aff = Ed.AffinityManager.GetAffinity(sel.Caret);
            var sym = aff.GetAutocompleteSymbols(Ed);
            return (sym != null ? sym.IndexOf(c) != -1 : false) || char.IsLetter(c);
        }

        public override ActionResults Redo(out Pos pos)
        {
            insertString = null;
            deleteChar = Character.Empty;
            Execute(redoSel, insertChar);
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
            return new InsertCharCommand();
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => true;
    }
}
