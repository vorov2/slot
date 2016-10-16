using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.None)]
    internal sealed class ClearSelectionCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            context.Document.Selections.Clear();
        }
    }
}
