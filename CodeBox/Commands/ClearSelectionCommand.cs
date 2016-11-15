using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class ClearSelectionCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            Buffer.Selections.Truncate();
            return Clean;
        }
    }
}
