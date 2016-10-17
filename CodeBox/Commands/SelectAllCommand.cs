using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(ClearSelections)]
    internal sealed class SelectAllCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var idx = context.Document.Lines.Count - 1;
            var ln = context.Document.Lines[idx];
            sel.Start = new Pos(0, 0);
            sel.End = new Pos(idx, ln.Length);
        }
    }
}
