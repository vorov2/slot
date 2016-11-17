using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.selectwordadd")]
    public class SelectWordAddCommand : SelectWordCommand
    {
        protected override void Select(Range range)
        {
            if (Buffer.Selections.Count == 1 && Buffer.Selections.Main.IsEmpty)
                base.Select(range);
            else
                Buffer.Selections.Add(Selection.FromRange(range));
        }
    }
}
