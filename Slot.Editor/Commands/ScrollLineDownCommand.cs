using System;
using Slot.Editor.ObjectModel;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.scrolldown")]
    public sealed class ScrollLineDownCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            Ed.Scroll.ScrollY(-1);
            return Clean | AutocompleteKeep;
        }
    }
}
