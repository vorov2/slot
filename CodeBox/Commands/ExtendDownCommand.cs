using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => DownCommand.MoveDown(Context, sel);
    }
}
