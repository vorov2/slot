using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    public sealed class DocumentEndCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos caret)
        {
            var idx = Document.Lines.Count - 1;
            return new Pos(idx, Document.Lines[idx].Length);
        }
    }
}
