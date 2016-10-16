using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return HomeCommand.MoveHome(context.Document, pos);
        }
    }
}
