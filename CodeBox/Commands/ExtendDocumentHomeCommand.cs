using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extenddocumenthome")]
    public sealed class ExtendDocumentHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => default(Pos);
    }
}
