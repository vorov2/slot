using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.overtype")]
    public sealed class OvertypeCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            View.Buffer.Overtype = !View.Buffer.Overtype;
            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
