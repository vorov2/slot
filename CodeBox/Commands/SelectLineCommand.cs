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
    public sealed class SelectLineCommand : Command
    {
        public override ActionResult Execute(CommandArgument arg, Selection sel)
        {
            if (arg.Pos.Line > -1)
            {
                sel.Start = new Pos(arg.Pos.Line, 0);
                sel.End = new Pos(arg.Pos.Line, Document.Lines[arg.Pos.Line].Length);
            }

            return ActionResult.Standard;
        }
    }
}
