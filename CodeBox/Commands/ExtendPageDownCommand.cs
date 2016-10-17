using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendPageDownCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return PageDownCommand.PageDown(context);
        }
    }
}
