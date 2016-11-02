using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(None)]
    public sealed class ScrollLineUpCommand : Command
    {
        public override ActionChange Execute(CommandArgument arg, Selection sel)
        {
            Context.Scroll.ScrollY(1);
            return ActionChange.None;
        }
    }
}
