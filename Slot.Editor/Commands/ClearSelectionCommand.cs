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
            if (Ed.Search.IsSearchVisible)
                Ed.Search.HideSearch();
            else
            {
                Buffer.Selections.Truncate();
                Buffer.Selections.Main.Clear();
            }

            if (!Ed.Focused)
                Ed.Focus();

            if (Ed.HasEscape)
                Ed.OnEscape();

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
