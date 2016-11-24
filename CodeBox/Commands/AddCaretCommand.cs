using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
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
