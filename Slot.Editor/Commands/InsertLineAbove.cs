﻿using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.insertlineabove")]
    public sealed class InsertLineAboveCommand : InsertNewLineCommand
    {
        private Selection redoSel;

        internal override ActionResults Execute(Selection selection, params object[] args)
        {
            redoSel = selection.Clone();
            var lni = selection.Caret.Line;
            var sel = new Selection(
                new Pos(selection.Caret.Line - 1, 
                    selection.Caret.Line > 0 ? Document.Lines[selection.Caret.Line - 1].Length : 0));
            var res = base.Execute(sel, args);
            Document.Lines[lni].State = 1;
            undoPos = selection.Caret;
            return res;
        }

        public override ActionResults Redo(out Pos pos)
        {
            var sel = redoSel;
            Execute(sel);
            pos = sel.Caret;
            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new InsertLineAboveCommand();
        }

        internal override bool ModifyContent => true;
    }
}
