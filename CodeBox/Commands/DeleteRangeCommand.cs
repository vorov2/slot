using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public class DeleteRangeCommand : Command
    {
        protected IEnumerable<Character> data;
        private Pos undoPos;
        private Selection redoSel;

        public override void Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();
            data = DeleteRange(Context, sel);
            undoPos = sel.Caret;
        }

        public override Pos Redo()
        {
            data = null;
            Execute(default(CommandArgument), redoSel);
            return undoPos;
        }

        public override Pos Undo()
        {
            InsertRangeCommand.InsertRange(Document, undoPos, data);
            Buffer.Selections.Set(undoPos);
            return undoPos;
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
    }
}
