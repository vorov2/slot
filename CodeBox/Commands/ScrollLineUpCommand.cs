using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.scrollup")]
    public sealed class ScrollLineUpCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            Context.Scroll.ScrollY(1);
            return Clean | AutocompleteKeep;
        }
    }
}
