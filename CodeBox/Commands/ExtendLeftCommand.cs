using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendLeftCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return LeftCommand.MoveLeft(Document, pos);
        }
    }
}
