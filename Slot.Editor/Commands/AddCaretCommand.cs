using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.addcaret")]
    public sealed class AddCaretCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var newSel = new Selection(View.Caret);
            Buffer.Selections.AddFast(newSel);

            var osel = Buffer.Selections.GetIntersection(newSel);

            if (osel != null)
                Buffer.Selections.Remove(osel);

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
