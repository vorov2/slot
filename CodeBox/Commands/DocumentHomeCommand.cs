using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    public sealed class DocumentHomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos caret)
        {
            return default(Pos);
        }
    }
}
