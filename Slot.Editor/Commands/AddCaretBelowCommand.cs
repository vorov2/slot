using System;
using System.Linq;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public sealed class AddCaretBelowCommand : EditorCommand
    {
        public const string Name = "editor.addCaretBelow";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            sel = Buffer.Selections.OrderByDescending(s => s.Start > s.End ? s.End : s.Start).First();
            var pos = DownCommand.MoveDown(View, sel);

            if (pos != sel.Caret)
            {
                var newSel = new Selection(pos);
                Buffer.Selections.Add(newSel, View.Document);
                newSel.SetToRestore(sel.RestoreCaretCol);
            }

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
