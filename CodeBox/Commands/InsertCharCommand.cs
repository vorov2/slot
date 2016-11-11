using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public class InsertCharCommand : Command, IModifyContent
    {
        private Character @char;
        private Character @redoChar;
        private IEnumerable<Character> @string;
        private Pos undoPos;
        private Selection redoSel;

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var line = Document.Lines[sel.Caret.Line];
            undoPos = sel.Start;
            redoSel = sel.Clone();
            var res = Change;

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(Context, sel);
            else if (Buffer.Overtype && sel.Caret.Col < line.Length)
            {
                res |= AtomicChange;
                @char = line[sel.Caret.Col];
                line.RemoveAt(sel.Caret.Col);
            }
            else
                res |= AtomicChange | AutocompleteKeep;

            @redoChar = new Character(arg.Char);
            Document.Lines[sel.Caret.Line].Insert(sel.Caret.Col, @redoChar);
            sel.Clear(new Pos(sel.Caret.Line, sel.Caret.Col + 1));
            return res;
        }

        public override ActionResults Redo(out Pos pos)
        {
            @string = null;
            @char = Character.Empty;
            var arg = new CommandArgument(redoChar.Char);
            redoChar = Character.Empty;
            Execute(arg, redoSel);
            pos = new Pos(redoSel.Start.Line, redoSel.Start.Col + 1);
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            var lines = Document.Lines;
            lines[undoPos.Line].RemoveAt(undoPos.Col);
            pos = Pos.Empty;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);

            if (!@char.IsEmpty)
                lines[undoPos.Line].Insert(undoPos.Col, @char);

            if (pos.IsEmpty)
                pos = undoPos;

            return Change;
        }

        public override ICommand Clone()
        {
            return new InsertCharCommand();
        }
    }
}
