using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class DocumentHomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => default(Pos);
    }
}
