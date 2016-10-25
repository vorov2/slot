using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return EndCommand.MoveEnd(Document, pos);
        }
    }
}
