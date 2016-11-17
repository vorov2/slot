using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.caretset")]
    public sealed class SetCaretCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            Buffer.Selections.Set(new Selection(Context.Caret));
            return Clean;
        }

        public override bool SingleRun => true;
    }
}
