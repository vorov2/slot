using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendPageUpCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return PageUpCommand.PageUp(Context);
        }
    }
}
