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
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.clearselections")]
    public sealed class ClearSelectionCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            Buffer.Selections.Truncate();
            return Clean;
        }
    }
}
