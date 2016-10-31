using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public abstract class CaretCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            var pos = GetPosition(sel);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel, Document);
            return true;
        }

        protected abstract Pos GetPosition(Selection sel);
    }
}
