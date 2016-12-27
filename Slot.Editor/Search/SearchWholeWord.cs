using Slot.Editor.Commands;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchwholeword")]
    public sealed class SearchWholeWordCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            Ed.Search.WholeWord = !Ed.Search.WholeWord;
            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
