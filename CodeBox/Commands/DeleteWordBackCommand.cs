using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;
using CodeBox.Affinity;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.deletewordback")]
    public sealed class DeleteWordBackCommand : DeleteBackCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == 0)
                return base.Execute(sel);

            var aff = View.AffinityManager.GetAffinity(sel.Caret);
            var seps = aff.NonWordSymbols ?? View.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col - 1));
            var col = SelectWordCommand.FindBoundLeft(seps, ln, sel.Caret.Col - 1, st);
            sel.End = new Pos(sel.Caret.Line, col != 0 ? col + 1 : col);
            return base.Execute(sel);
        }

        public override ActionResults Redo(out Pos pos)
        {
            Execute(new Selection(redoSel.Start));
            pos = redoSel.Caret;
            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new DeleteWordBackCommand();
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => true;
    }
}
