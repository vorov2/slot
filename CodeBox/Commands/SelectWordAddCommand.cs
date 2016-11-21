using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.selectwordadd", "eswa")]
    public class SelectWordAddCommand : SelectWordCommand
    {
        protected override void Select(Range range)
        {
            if (Buffer.Selections.Count == 1 && Buffer.Selections.Main.IsEmpty)
                base.Select(range);
            else
                Buffer.Selections.AddFast(Selection.FromRange(range));
        }
    }
}
