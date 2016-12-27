using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendwordleft")]
    public sealed class ExtendWordLeftCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordLeftCommand.WordLeft(Ed, sel);

        internal override bool SupportLimitedMode => true;
    }
}
