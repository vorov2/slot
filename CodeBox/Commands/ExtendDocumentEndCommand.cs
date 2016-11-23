using System;
using CodeBox.ObjectModel;
using System.ComponentModel.Composition;
using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extenddocumentend")]
    public sealed class ExtendDocumentEndCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }
    }
}
