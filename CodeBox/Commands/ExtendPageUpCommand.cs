using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendPageUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageUpCommand.PageUp(Context);
    }
}
