using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class NormalSelectCommand : Command
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            Console.WriteLine("NornmalSelectCommand");
            DoSelection(arg.Pos);
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
