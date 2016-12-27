using Slot.Editor.Commands;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchregex")]
    public sealed class SearchRegexCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            Ed.Search.UseRegex = !Ed.Search.UseRegex;
            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
