using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.autocomplete", "eea")]
    public sealed class AutocompleteCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            return Pure | AutocompleteShow;
        }

        internal override bool SingleRun => true;
    }
}
