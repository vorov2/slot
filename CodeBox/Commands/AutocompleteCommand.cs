using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.autocomplete")]
    public sealed class AutocompleteCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            return Pure | AutocompleteShow;
        }

        public override bool SingleRun => true;
    }
}
