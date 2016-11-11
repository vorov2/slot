using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordRightCommand.WordRight(Context, sel);
    }
}
