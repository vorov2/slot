using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendWordLeftCommandCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return WordLeftCommand.WordLeft(Context, pos);
        }
    }
}
