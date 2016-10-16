using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll)]
    internal sealed class ExtendDocumentEndCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            var ln = context.Document.Lines[context.Document.Lines.Count - 1];
            return new Pos(ln.Index, ln.Length);
        }
    }
}
