using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return EndCommand.MoveEnd(context.Document, pos);
        }
    }
}
