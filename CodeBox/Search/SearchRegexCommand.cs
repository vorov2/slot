using CodeBox.Commands;
using CodeBox.ObjectModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchregex")]
    public sealed class SearchRegexCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            View.Search.UseRegex = !View.Search.UseRegex;
            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
