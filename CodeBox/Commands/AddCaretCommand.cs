using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class AddCaretCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Add(new Selection(arg.Pos));
            return SingleRun | Clean;
        }
    }
}
