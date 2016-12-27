using System;
using Slot.Editor.ObjectModel;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.setcaret")]
    public sealed class SetCaretCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var newsel = default(Selection);
            var range = default(Range);

            if (!sel.IsEmpty && (range = SelectWordCommand.SelectWord(Ed, Ed.Caret)) != null
                && range.Start == sel.Start && range.End == sel.End)
            {
                newsel = new Selection(new Pos(sel.Caret.Line, 0),
                    new Pos(sel.Caret.Line, Document.Lines[sel.Caret.Line].Length));
            }
            else
                newsel = new Selection(Ed.Caret);

            Buffer.Selections.Set(newsel);
            return Clean;
        }

        internal override bool SingleRun => true;

        internal override bool SupportLimitedMode => true;
    }
}
