using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendPageDownCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return PageDownCommand.PageDown(context);
        }
    }
}
