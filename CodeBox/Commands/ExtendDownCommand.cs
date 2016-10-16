using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return DownCommand.MoveDown(context, pos);
        }
    }
}
