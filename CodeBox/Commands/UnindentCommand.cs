using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using System.Linq;
using System.Collections.Generic;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.unindent", "elu")]
    public sealed class UnindentCommand : EditorCommand
    {
        private Selection redoSel;
        private List<int> undoIndents;
        private bool useTab;

        protected override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            useTab = View.UseTabs;
            undoIndents = Unindent(View, sel);

            if (undoIndents.Sum() > 0)
            {
                ShiftSel(sel);
                return Modify | Scroll;
            }
            else
                return Clean;
        }

        public override ActionResults Redo(out Pos pos)
        {
            pos = redoSel.Caret;

            if (undoIndents != null)
            {
                var norm = redoSel.Normalize();
                var c = norm.Start.Line;

                foreach (var i in undoIndents)
                {
                    Document.Lines[c++].RemoveRange(0, i);

                    if (c - 1 == pos.Line)
                        pos = new Pos(pos.Line, pos.Col - i);
                }

                return Change;
            }
            else
                return Clean;
        }

        public override ActionResults Undo(out Pos pos)
        {
            pos = Pos.Empty;

            if (undoIndents != null)
            {
                var norm = redoSel.Normalize();
                var c = norm.Start.Line;
                pos = redoSel.Caret;

                foreach (var i in undoIndents)
                {
                    var str = new string(useTab ? '\t' : ' ', i);
                    Document.Lines[c++].Insert(0, str.MakeCharacters());
                }

                return Change;
            }
            else
                return Clean;
        }

        private void ShiftSel(Selection sel)
        {
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - undoIndents[0]);
            sel.End = new Pos(sel.End.Line, sel.End.Col - undoIndents[undoIndents.Count - 1]);
        }

        internal override EditorCommand Clone()
        {
            return new UnindentCommand();
        }

        internal static List<int> Unindent(IEditorView ctx, Selection sel)
        {
            var norm = sel.Normalize();
            var indents = new List<int>();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                var col = line.GetFirstNonIndentChar();

                if (col == 0)
                {
                    indents.Add(0);
                    continue;
                }

                var indent = Line.GetIndentationSize(line.GetTetras(col, ctx.IndentSize), ctx.IndentSize);
                var unindent = indent == ctx.IndentSize ? indent : ctx.IndentSize - indent;
                indents.Add(unindent);
                line.RemoveRange(0, unindent);
            }

            return indents;
        }


        public override bool ModifyContent => true;
    }
}
