using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.overtype", "edo")]
    public sealed class OvertypeCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            View.Overtype = !View.Overtype;
            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
