using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendpageup")]
    public sealed class ExtendPageUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageUpCommand.PageUp(Ed);
    }
}
