using Slot.Editor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.showsearch")]
    public sealed class ShowSearchCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!Ed.Search.IsFocused)
                Ed.Search.ShowSearch();
            else
                Ed.Focus();

            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
