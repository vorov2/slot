using System;
using System.Linq;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public sealed class AddCaretAboveCommand : EditorCommand
    {
        public const string Name = "editor.addCaretAbove";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            sel = Buffer.Selections.OrderBy(s => s.Start > s.End ? s.End : s.Start).First();
            var pos = UpCommand.MoveUp(View, sel);

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
