using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public sealed class InsertNewLineCommand : Command
    {
        private IEnumerable<Character> @string;
        private Pos undoPos;
        private Selection redoSel;
        private int indent;
        private IEnumerable<Character> unindent;

        public override ActionResults Execute(CommandArgument arg, Selection selection)
        {
            undoPos = selection.Start;
            redoSel = selection.Clone();

            if (!selection.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(Context, selection);
            
            var pos = InsertNewLine(Document, undoPos);
            selection.Clear(pos);
            indent = Context.Indents.CalculateIndentation(pos.Line);

            if (indent > 0)
            {
                if (pos.Line > 0)
                {
                    var ln = Document.Lines[pos.Line - 1];

                    if (ln.IsEmpty() && ln.Length > 0)
                    {
                        unindent = ln.ToList();
                        ln.RemoveRange(0, ln.Length);
                    }
                }

                var str = Context.Settings.UseTabs ? new string('\t', indent / Context.Settings.TabSize)
                    : new string(' ', indent);
                Document.Lines[pos.Line].Insert(0, str.MakeCharacters());
                selection.Clear(new Pos(pos.Line, pos.Col + str.Length));
            }

            return ActionResults.Change;
        }

        public override Pos Redo()
        {
            @string = null;
            var sel = redoSel;
            Execute(default(CommandArgument), sel);
            return sel.Caret;
        }

        public override Pos Undo()
        {
            var pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);

            var line = Document.Lines[pos.Line];
            var nextLine = Document.Lines[pos.Line + 1];
            Document.Lines.Remove(nextLine);
            
            if (indent > 0)
                nextLine.RemoveRange(0, indent);

            if (unindent != null)
                line.Insert(0, unindent);

            line.Append(nextLine);
            return pos;
        }

        internal static Pos InsertNewLine(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            var str = default(IEnumerable<Character>);

            if (pos.Col != ln.Length && pos.Col < ln.Length)
            {
                str = ln.GetRange(pos.Col, ln.Length - pos.Col);
                ln.RemoveRange(pos.Col, ln.Length - pos.Col);
            }

            var newLn = doc.NewLine(str);
            doc.Lines.Insert(pos.Line + 1, newLn);
            pos = new Pos(pos.Line + 1, 0);
            return pos;
        }
    }
}
