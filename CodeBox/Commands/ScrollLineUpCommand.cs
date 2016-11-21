using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.scrollup", "emu")]
    public sealed class ScrollLineUpCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            View.Scroll.ScrollY(1);
            return Clean | AutocompleteKeep;
        }
    }
}
