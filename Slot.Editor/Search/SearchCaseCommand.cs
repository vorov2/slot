using Slot.Editor.Commands;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchcasesensitive")]
    public sealed class SearchCaseCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            View.Search.CaseSensitive = !View.Search.CaseSensitive;
            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
