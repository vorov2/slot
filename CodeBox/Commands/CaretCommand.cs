using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public abstract class CaretCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var pos = GetPosition(sel);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel, Document);
            return ActionResults.Clean;
        }

        protected abstract Pos GetPosition(Selection sel);
    }
}
