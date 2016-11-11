using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class DocumentEndCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }
    }
}
