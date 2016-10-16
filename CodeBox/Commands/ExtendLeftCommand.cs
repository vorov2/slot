using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendLeftCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return LeftCommand.MoveLeft(context.Document, pos);
        }
    }
}
