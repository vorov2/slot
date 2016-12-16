using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
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
