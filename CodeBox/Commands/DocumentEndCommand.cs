using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal sealed class DocumentEndCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            var idx = context.Document.Lines.Count - 1;
            return new Pos(idx, context.Document.Lines[idx].Length);
        }
    }
}
