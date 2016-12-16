using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;

namespace Slot.Editor.Commands
{
    public abstract class SelectionCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var pos = Select(sel);
            sel.End = pos;
            return Clean | Scroll;
        }

        protected abstract Pos Select(Selection sel);
    }
}
