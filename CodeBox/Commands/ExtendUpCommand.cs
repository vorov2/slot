using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return UpCommand.MoveUp(context, pos);
        }
    }
}
