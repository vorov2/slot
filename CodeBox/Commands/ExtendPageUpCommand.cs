using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendPageUpCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return PageUpCommand.PageUp(context);
        }
    }
}
