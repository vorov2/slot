using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.selectnormal")]
    public sealed class NormalSelectCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            DoSelection(Context.Caret);
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

        public override bool SingleRun => true;
    }
}
