using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendPageDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageDownCommand.PageDown(Context);
    }
}
