using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return UpCommand.MoveUp(context, pos);
        }
    }
}
