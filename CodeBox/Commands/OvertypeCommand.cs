using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun)]
    public sealed class OvertypeCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            Context.Overtype = !Context.Overtype;
            return ActionResults.Clean;
        }
    }
}
