using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extenddown")]
    public sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => DownCommand.MoveDown(Ed, sel);
    }
}
