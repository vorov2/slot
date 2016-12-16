using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendright")]
    public sealed class ExtendRightCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => RightCommand.MoveRight(View, sel);

        internal override bool SupportLimitedMode => true;
    }
}
