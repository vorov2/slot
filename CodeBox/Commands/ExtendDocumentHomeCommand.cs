using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendDocumentHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => default(Pos);
    }
}
