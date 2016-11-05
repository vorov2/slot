using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendLeftCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => LeftCommand.MoveLeft(Document, sel);
    }
}
