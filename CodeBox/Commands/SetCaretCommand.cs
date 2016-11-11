using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class SetCaretCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Set(new Selection(arg.Pos));
            return Clean | SingleRun;
        }
    }
}
