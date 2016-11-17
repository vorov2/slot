using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.caretadd")]
    public sealed class AddCaretCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            var newSel = new Selection(Context.Caret);
            Buffer.Selections.Add(newSel);

            var osel = Buffer.Selections.GetIntersection(newSel);

            if (osel != null)
                Buffer.Selections.Remove(osel);

            return Clean;
        }

        public override bool SingleRun => true;
    }
}
