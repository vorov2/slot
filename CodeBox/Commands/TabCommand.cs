using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.Undoable)]
    internal sealed class TabCommand : InsertRangeCommand
    {
        private bool undoIndent;

        public override void Execute(EditorContext context, Selection sel)
        {
            var indent = context.Settings.UseTabs ? "\t" : new string(' ', context.Settings.TabSize);
            
            if (sel.Start.Line != sel.End.Line)
            {
                undoIndent = true;
                redoSel = sel.Clone();
                var str = indent.MakeCharacters();
                Indent(context, sel, str);
            }
            else
            {
                context.String = indent;
                base.Execute(context, sel);
            }
        }

        public override Pos Redo(EditorContext context)
        {
            if (undoIndent)
            {
                var sel = redoSel;
                Execute(context, sel);
                return sel.End;
            }
            return base.Redo(context);
        }

        public override Pos Undo(EditorContext context)
        {
            if (undoIndent)
            {
                Unindent(context, redoSel);
                return redoSel.Caret;
            }
            else
                return base.Undo(context);
        }

        internal static void Unindent(EditorContext context, Selection sel)
        {
            var norm = sel.Normalize();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = context.Document.Lines[i];
                var pos = HomeCommand.MoveHome(context.Document, new Pos(i, line.Length));

                if (pos.Col == 0)
                    continue;
                else if (line[pos.Col - 1].Char == '\t')
                    line.RemoveAt(pos.Col - 1);
                else
                {
                    var st = pos.Col - context.Settings.TabSize;
                    st = st < 0 ? 0 : st;
                    line.RemoveRange(st, pos.Col - st);
                }
            }
        }

        internal static void Indent(EditorContext context, Selection sel, IEnumerable<Character> indent)
        {
            var norm = sel.Normalize();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = context.Document.Lines[i];
                var pos = HomeCommand.MoveHome(context.Document, new Pos(i, line.Length));
                line.Insert(pos.Col, indent);
            }

            sel.Start = new Pos(sel.Start.Line, sel.Start.Col + indent.Count());
            sel.End = new Pos(sel.End.Line, sel.End.Col + indent.Count());
        }
    }
}
