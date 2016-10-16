using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendDocumentHomeCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            return new Pos(0, 0);
        }
    }
}
