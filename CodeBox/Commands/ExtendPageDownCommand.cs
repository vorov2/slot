using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendpagedown")]
    public sealed class ExtendPageDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageDownCommand.PageDown(View);
    }
}
