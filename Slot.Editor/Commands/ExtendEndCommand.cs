using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendend")]
    public sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => EndCommand.MoveEnd(Ed, sel.Caret);

        internal override bool SupportLimitedMode => true;
    }
}
