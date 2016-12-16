using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.documentend")]
    public sealed class DocumentEndCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }

        internal override bool SupportLimitedMode => true;
    }
}
