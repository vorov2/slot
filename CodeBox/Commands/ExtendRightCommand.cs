using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendRightCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => RightCommand.MoveRight(Context, sel);
    }
}
