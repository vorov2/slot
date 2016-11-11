using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class NormalSelectCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            DoSelection(arg.Pos);
            return Clean | Scroll | SingleRun;
        }

        private void DoSelection(Pos p)
        {
            var sel = Buffer.Selections[Buffer.Selections.Count - 1];
            sel.End = p;
            var osel = Buffer.Selections.GetSelection(p, sel);

            if (osel != null)
                Buffer.Selections.Remove(osel);
        }
    }
}
