using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal sealed class DocumentEndCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            var ln = context.Document.Lines[context.Document.Lines.Count - 1];
            return new Pos(ln.Index, ln.Length);
        }
    }
}
