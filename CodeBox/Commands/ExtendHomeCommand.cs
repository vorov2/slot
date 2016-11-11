using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => HomeCommand.MoveHome(Document, sel.Caret);
    }
}
