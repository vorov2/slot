using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.overtype")]
    public sealed class OvertypeCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            Context.Overtype = !Context.Overtype;
            return Clean;
        }

        public override bool SingleRun => true;
    }
}
