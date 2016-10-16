using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return WordRightCommand.WordRight(context, pos);
        }
    }
}
