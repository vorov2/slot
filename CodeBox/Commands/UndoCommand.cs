using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class UndoCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Context.Commands.Undo();
            return SingleRun | Pure;
        }
    }
}
