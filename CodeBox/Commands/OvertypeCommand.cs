using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class OvertypeCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            Context.Overtype = !Context.Overtype;
            return Clean | SingleRun;
        }
    }
}
