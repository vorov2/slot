using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class ShiftTabCommand : Command, IModifyContent
    {
        private Selection redoSel;

        public override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var change = TabCommand.Unindent(Context, sel);

            if (change)
            {
                ShiftSel(sel);
                return Modify | Scroll;
            }
            else
                return Clean;
        }

        public override ActionResults Redo(out Pos pos)
        {
            var sel = redoSel;
            Execute(sel);
            pos = sel.End;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            var indent = Context.UseTabs ? "\t" : new string(' ', Context.TabSize);
            TabCommand.Indent(Context, redoSel, indent.MakeCharacters());
            ShiftSel(redoSel);
            pos = redoSel.Caret;
            return Change;
        }

        private void ShiftSel(Selection sel)
        {
            var indent = Context.UseTabs ? 1 : Context.TabSize;
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - indent);
            sel.End = new Pos(sel.End.Line, sel.End.Col - indent);
        }

        public override ICommand Clone()
        {
            return new ShiftTabCommand();
        }
    }
}
