using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.caretset", "ecs")]
    public sealed class SetCaretCommand : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            var newsel = default(Selection);
            var range = default(Range);

            if (!sel.IsEmpty && (range = SelectWordCommand.SelectWord(View, View.Caret)) != null
                && range.Start == sel.Start && range.End == sel.End)
            {
                newsel = new Selection(new Pos(sel.Caret.Line, 0),
                    new Pos(sel.Caret.Line, Document.Lines[sel.Caret.Line].Length));
            }
            else
                newsel = new Selection(View.Caret);

            Buffer.Selections.Set(newsel);
            return Clean;
        }

        public override bool SingleRun => true;

        public override bool SupportLimitedMode => true;
    }
}
