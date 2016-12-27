using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendup")]
    public sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => UpCommand.MoveUp(Ed, sel);
    }
}
