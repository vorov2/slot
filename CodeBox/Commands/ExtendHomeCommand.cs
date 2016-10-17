using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return HomeCommand.MoveHome(context.Document, pos);
        }
    }
}
