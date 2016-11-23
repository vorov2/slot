using System;
using CodeBox.ObjectModel;
using CodeBox.Autocomplete;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.autocomplete")]
    public sealed class AutocompleteCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            return Pure | AutocompleteShow;
        }

        internal override bool SingleRun => true;
    }
}
