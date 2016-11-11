using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => UpCommand.MoveUp(Context, sel);
    }
}
