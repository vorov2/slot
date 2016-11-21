using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.selectnormal", "esn")]
    public sealed class NormalSelectCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            DoSelection(View.Caret);
            return Clean | Scroll;
        }

        private void DoSelection(Pos p)
        {
            var sel = Buffer.Selections[Buffer.Selections.Count - 1];
            sel.End = p;
            var osel = Buffer.Selections.GetIntersection(sel);

            if (osel != null)
                Buffer.Selections.Remove(osel);
        }

        internal override bool SingleRun => true;

        internal override bool SupportLimitedMode => true;
    }
}
