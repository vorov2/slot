using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.scrolldown", "emd")]
    public sealed class ScrollLineDownCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            View.Scroll.ScrollY(-1);
            return Clean | AutocompleteKeep;
        }
    }
}
