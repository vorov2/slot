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
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.selectionclear")]
    public sealed class ClearSelectionCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            Buffer.Selections.Truncate();
            return Clean;
        }
    }
}
