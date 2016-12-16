using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public class DeleteAllLeftCommand : DeleteRangeCommand
    {
        public const string Name = "editor.deleteAllLeft";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!sel.IsEmpty)
                return base.Execute(sel, args);
            else if (sel.Caret.Col == 0)
                return Pure;
            else
            {
                sel.Start = new Pos(sel.Caret.Line, 0);
                sel.End = sel.Caret;
                return base.Execute(sel, args);
            }
        }

        internal override EditorCommand Clone()
        {
            return new DeleteAllLeftCommand();
        }

        internal override bool ModifyContent => true;
    }
}
