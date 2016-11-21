using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.autocomplete", "eea")]
    public sealed class AutocompleteCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            return Pure | AutocompleteShow;
        }

        public override bool SingleRun => true;
    }
}
