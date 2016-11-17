using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public class DeleteRangeCommand : EditorCommand
    {
        protected IEnumerable<Character> data;
        private Pos undoPos;
        private Selection redoSel;

        public override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            data = DeleteRange(Context, sel);
            undoPos = sel.Caret;
            return data != null ? Change : Pure;
        }

        public override ActionResults Redo(out Pos pos)
        {
            data = null;
            Execute(redoSel);
            pos = undoPos;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            undoPos = InsertRangeCommand.InsertRange(Document, undoPos, data);
            pos = undoPos;
            return Change;
        }

        internal override EditorCommand Clone()
        {
            return new DeleteRangeCommand();
        }

        internal static IEnumerable<Character> DeleteRange(IEditorContext ctx, Selection selection)
        {
            var doc = ctx.Buffer.Document;
            var sel = selection.Normalize();

            if (sel.IsEmpty)
                return null;

            var range = new List<Character>();

            if (sel.Start.Line == sel.End.Line)
            {
                range = doc.Lines[sel.Start.Line]
                    .GetRange(sel.Start.Col, sel.End.Col - sel.Start.Col);
                doc.Lines[sel.Start.Line].RemoveRange(
                    sel.Start.Col, sel.End.Col - sel.Start.Col);
            }
            else
            {
                var startLine = doc.Lines[sel.Start.Line];
                var endLine = doc.Lines[sel.End.Line];
                range = startLine.GetRange(sel.Start.Col, startLine.Length - sel.Start.Col);
                startLine.RemoveRange(sel.Start.Col, startLine.Length - sel.Start.Col);
                var len = endLine.Length - sel.End.Col;

                if (len > 0)
                    startLine.Append(endLine.GetRange(sel.End.Col, len));

                if (sel.End.Line - sel.Start.Line > 0)
                {
                    range.Add(Character.NewLine);

                    for (var i = sel.Start.Line + 1; i < sel.End.Line; i++)
                    {
                        range.AddRange(doc.Lines[i]);
                        range.Add(Character.NewLine);
                    }

                    doc.Lines.RemoveRange(sel.Start.Line + 1, sel.End.Line - sel.Start.Line - 1);
                    range.AddRange(endLine.GetRange(0, sel.End.Col));
                    doc.Lines.Remove(endLine);
                }
            }

            selection.Clear(sel.Start);
            return range;
        }

        public override bool ModifyContent => true;
    }
}
