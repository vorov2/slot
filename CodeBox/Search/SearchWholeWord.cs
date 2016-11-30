using CodeBox.Commands;
using CodeBox.ObjectModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchwholeword")]
    public sealed class SearchWholeWordCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            View.Search.WholeWord = !View.Search.WholeWord;
            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
