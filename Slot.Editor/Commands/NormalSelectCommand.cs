using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.selectnormal")]
    public sealed class NormalSelectCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
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
