using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(ClearSelections)]
    public sealed class SelectAllCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            var idx = Document.Lines.Count - 1;
            var ln = Document.Lines[idx];
            sel.Start = default(Pos);
            sel.End = new Pos(idx, ln.Length);
            return true;
        }
    }
}
