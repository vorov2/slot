using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.overtype", "edo")]
    public sealed class OvertypeCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            View.Overtype = !View.Overtype;
            return Clean;
        }

        public override bool SingleRun => true;
    }
}
