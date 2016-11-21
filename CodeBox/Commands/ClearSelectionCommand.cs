using CodeBox.ComponentModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.selectionclear", "esc")]
    public sealed class ClearSelectionCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            Buffer.Selections.Truncate();
            return Clean;
        }
    }
}
