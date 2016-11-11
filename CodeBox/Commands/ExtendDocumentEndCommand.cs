using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendDocumentEndCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }
    }
}
