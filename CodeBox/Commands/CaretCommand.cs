using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    internal abstract class CaretCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            var pos = GetPosition(sel.Caret);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel, Document);
        }

        protected abstract Pos GetPosition(Pos caret);
    }
}
