using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    internal class InsertCharCommand : Command //Tested
    {
        private Character @char;
        private Character @redoChar;
        private IEnumerable<Character> @string;
        private Pos undoPos;
        private Selection redoSel;

        public override void Execute(EditorContext context, Selection sel)
        {
            var line = context.Document.Lines[sel.Caret.Line];
            undoPos = sel.Start;
            redoSel = sel.Clone();

            if (!sel.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(context, sel);
            else if (context.Overtype)
            {
                @char = line[sel.Caret.Col];
                line.RemoveAt(sel.Caret.Col);
            }

            @redoChar = new Character(context.Char);
            context.Document.Lines[sel.Caret.Line].Insert(sel.Caret.Col, @redoChar);
            sel.Clear(new Pos(sel.Caret.Line, sel.Caret.Col + 1));
        }

        public override Pos Redo(EditorContext context)
        {
            @string = null;
            @char = Character.Empty;
            context.Char = @redoChar.Char;
            redoChar = Character.Empty;
            Execute(context, redoSel);
            return new Pos(redoSel.Caret.Line, redoSel.Caret.Col + 1);
        }

        public override Pos Undo(EditorContext context)
        {
            var lines = context.Document.Lines;
            lines[undoPos.Line].RemoveAt(undoPos.Col);
            var pos = Pos.Empty;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(context.Document, undoPos, @string);

            if (!@char.IsEmpty)
                lines[undoPos.Line].Insert(undoPos.Col, @char);

            if (pos.IsEmpty)
                pos = undoPos;

            context.Document.Selections.Set(pos);
            return pos;
        }
    }
}
