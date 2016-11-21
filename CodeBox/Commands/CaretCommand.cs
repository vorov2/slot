using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public abstract class CaretCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            var pos = GetPosition(sel);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel);
            return Clean | Scroll;
        }

        protected abstract Pos GetPosition(Selection sel);
    }
}
