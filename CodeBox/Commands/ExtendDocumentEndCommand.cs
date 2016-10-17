using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendDocumentEndCommand : SelectionCommand
    {
        protected override Pos Select(EditorContext context, Pos pos)
        {
            var idx = context.Document.Lines.Count - 1;
            return new Pos(idx, context.Document.Lines[idx].Length);
        }
    }
}
