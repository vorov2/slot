using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;

namespace Slot.Editor.Commands
{
    public abstract class CaretCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var pos = GetPosition(sel);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel);
            return Clean | Scroll;
        }

        protected abstract Pos GetPosition(Selection sel);
    }
}
