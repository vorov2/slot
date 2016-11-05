using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordRightCommand.WordRight(Context, sel);
    }
}
