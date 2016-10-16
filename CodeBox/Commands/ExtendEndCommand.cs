using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return EndCommand.MoveEnd(context.Document, pos);
        }
    }
}
