using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.ClearSelections)]
    internal sealed class SelectAllCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var ln = context.Document.Lines[context.Document.Lines.Count - 1];
            sel.Start = new Pos(0, 0);
            sel.End = new Pos(ln.Index, ln.Length);
        }
    }
}
