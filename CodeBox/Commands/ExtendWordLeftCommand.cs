using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendWordLeftCommandCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return WordLeftCommand.WordLeft(context, pos);
        }
    }
}
