using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendDocumentEndCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }
    }
}
