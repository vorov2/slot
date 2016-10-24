using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal sealed class DocumentHomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos caret)
        {
            return default(Pos);
        }
    }
}
