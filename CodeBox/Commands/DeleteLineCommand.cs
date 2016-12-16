using System;
using System.Collections.Generic;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.deleteline")]
    public class DeleteLineCommand : EditorCommand
    {
        private List<Line> deleteLines;
        private Selection redoSel;
        private Pos undoPos;

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            redoSel = sel.Clone();
            undoPos = redoSel.Caret;
            var norm = sel.Normalize();
            deleteLines = new List<Line>();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var ln = Document.Lines[i];
                ln.Reset();
                deleteLines.Add(ln);
            }

            foreach (var ln in deleteLines)
                Document.Lines.Remove(ln);

            if (Document.Lines.Count == 0)
                Document.Lines.Add(Line.Empty());

            var pos = new Pos(norm.Start.Line >= Document.Lines.Count ? Document.Lines.Count - 1 : norm.Start.Line, 0);
            sel.Clear(pos);
            return Change;
        }

        public override ActionResults Redo(out Pos pos)
        {
            var sel = redoSel;
            Execute(sel);
            pos = sel.Caret;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            pos = undoPos;
            var idx = redoSel.GetFirstLine();
            
            foreach (var ln in deleteLines)
                Document.Lines.Insert(idx++, ln);

            deleteLines = null;
            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new DeleteLineCommand();
        }

        internal override bool ModifyContent => true;
    }
}
