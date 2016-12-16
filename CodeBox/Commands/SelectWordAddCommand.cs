using System;
using Slot.Editor.ObjectModel;
using Slot.Editor.Affinity;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.selectwordadd")]
    public class SelectWordAddCommand : SelectWordCommand
    {
        protected override void Select(Range range)
        {
            if (Buffer.Selections.Count == 1 && Buffer.Selections.Main.IsEmpty)
                base.Select(range);
            else
                Buffer.Selections.AddFast(Selection.FromRange(range));
        }
    }
}
