using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendhome")]
    public sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => HomeCommand.MoveHome(View, sel.Caret);

        internal override bool SupportLimitedMode => true;
    }
}
