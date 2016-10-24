using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return WordRightCommand.WordRight(Context, pos);
        }
    }
}
