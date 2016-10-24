using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return HomeCommand.MoveHome(Document, pos);
        }
    }
}
