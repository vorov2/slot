using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.caretadd", "eca")]
    public sealed class AddCaretCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            var newSel = new Selection(View.Caret);
            Buffer.Selections.Add(newSel);

            var osel = Buffer.Selections.GetIntersection(newSel);

            if (osel != null)
                Buffer.Selections.Remove(osel);

            return Clean;
        }

        public override bool SingleRun => true;
    }
}
