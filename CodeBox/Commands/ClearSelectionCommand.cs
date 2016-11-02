using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(None)]
    public sealed class ClearSelectionCommand : Command
    {
        public override ActionResult Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Truncate();
            return ActionResult.Standard;
        }
    }
}
