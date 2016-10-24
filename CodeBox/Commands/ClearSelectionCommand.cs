using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(None)]
    internal sealed class ClearSelectionCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            Buffer.Selections.Truncate();
        }
    }
}
