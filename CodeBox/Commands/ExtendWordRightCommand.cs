using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendwordright")]
    public sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordRightCommand.WordRight(View, sel);

        internal override bool SupportLimitedMode => true;
    }
}
