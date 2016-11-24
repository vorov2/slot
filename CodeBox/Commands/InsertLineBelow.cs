using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using CodeBox.Affinity;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.insertlinebelow")]
    public sealed class InsertLineBelowCommand : InsertNewLineCommand
    {
        private Selection redoSel;

        internal override ActionResults Execute(Selection selection, params object[] args)
        {
            redoSel = selection.Clone();
            var sel = new Selection(
                new Pos(selection.Caret.Line, Document.Lines[selection.Caret.Line].Length));
            var res = base.Execute(sel, args);
            undoPos = selection.Caret;
            selection.Clear(sel.Caret);
            return res;
        }

        public override ActionResults Redo(out Pos pos)
        {
            var caret = redoSel.Caret;
            var sel = redoSel;
            Execute(sel);
            undoPos = caret;
            pos = sel.Caret;
            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new InsertLineBelowCommand();
        }

        internal override bool ModifyContent => true;
    }
}
