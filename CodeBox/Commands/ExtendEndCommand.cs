using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => EndCommand.MoveEnd(Document, sel.Caret);
    }
}
