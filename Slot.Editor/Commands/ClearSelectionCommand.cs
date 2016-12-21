using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.clearselections")]
    public sealed class ClearSelectionCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (View.Search.IsSearchVisible)
                View.Search.HideSearch();
            else
            {
                Buffer.Selections.Truncate();
                Buffer.Selections.Main.Clear();
            }

            if (!View.Focused)
                View.Focus();

            if (View.HasEscape)
                View.OnEscape();

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
