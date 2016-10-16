using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendRightCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return RightCommand.MoveRight(context, pos);
        }
    }
}
