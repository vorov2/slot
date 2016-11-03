using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | SingleRun)]
    public sealed class NormalSelectCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            DoSelection(arg.Pos);
            return ActionResults.Clean;
        }

        private void DoSelection(Pos p)
        {
            var sel = Buffer.Selections.Main;
            sel.End = p;
            var osel = Buffer.Selections.GetSelection(p, sel);

            if (osel != null)
                Buffer.Selections.Remove(osel);
        }
    }
}
