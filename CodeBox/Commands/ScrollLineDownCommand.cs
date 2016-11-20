using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.scrolldown")]
    public sealed class ScrollLineDownCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            View.Scroll.ScrollY(-1);
            return Clean | AutocompleteKeep;
        }
    }
}
