using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => DownCommand.MoveDown(Context, sel);
    }
}
