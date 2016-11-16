using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class TabCommand : InsertRangeCommand, IModifyContent
    {
        private bool undoIndent;

        public override ActionResults Execute(Selection sel)
        {
            var indent = Context.UseTabs ? "\t" : new string(' ', Context.IndentSize);
            
            if (sel.Start.Line != sel.End.Line)
            {
                undoIndent = true;
                redoSel = sel.Clone();
                var str = indent.MakeCharacters();
                Indent(Context, sel, str);
                return Modify | Scroll;
            }
            else
            {
                if (!Context.UseTabs)
                {
                    var newIndent = FindIndent(true, sel.Caret.Line, indent.Length);

                    if (newIndent == -1)
                        newIndent = FindIndent(false, sel.Caret.Line, indent.Length);

                    if (newIndent != -1)
                        indent = new string(' ', newIndent);
                }

                base.insertString = indent.MakeCharacters();
                base.Execute(sel);
                return Modify | Scroll;
            }
        }

        private int FindIndent(bool backward, int line, int indent)
        {
            var first = -1;
            var firstIndent = -1;
            
            for (var i = line
                ; backward ? i > -1 : i < Document.Lines.Count
                ; i += backward ? -1 : 1)
            {
                var sp = CountSpaces(i);

                if (first == -1)
                {
                    first = sp < indent ? indent : sp;
                    firstIndent = sp;
                }
                else if (sp > first)
                    return sp - firstIndent;
                else
                    return -1;
            }

            return -1;
        }

        private int CountSpaces(int line)
        {
            var ln = Document.Lines[line];
            var spaces = 0;

            foreach (var c in ln)
                if (c.Char == ' ')
                    spaces++;
                else if (c.Char == '\t')
                    spaces += Context.IndentSize;
                else
                    return spaces;

            return spaces;
        }

        public override ActionResults Redo(out Pos pos)
        {
            if (undoIndent)
            {
                var sel = redoSel;
                Execute(sel);
                pos = sel.End;
                return Change;
            }
            else
                return base.Redo(out pos);
        }

        public override ActionResults Undo(out Pos pos)
        {
            if (undoIndent)
            {
                var change = Unindent(Context, redoSel);
                pos = redoSel.Caret;
                return Change;
            }
            else
                return base.Undo(out pos);
        }

        internal static bool Unindent(IEditorContext ctx, Selection sel)
        {
            var norm = sel.Normalize();
            var change = false;

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                var pos = HomeCommand.MoveHome(ctx.Buffer.Document, new Pos(i, line.Length));

                if (pos.Col == 0 && line.All(c => c.Char == ' ' || c.Char == '\t'))
                    pos = new Pos(pos.Line, line.Length);
                else if (pos.Col == 0 && (line.CharAt(0) == ' ' || line.CharAt(0) == '\t'))
                    pos = HomeCommand.MoveHome(ctx.Buffer.Document, pos);

                if (pos.Col == 0)
                    continue;
                else if (line[pos.Col - 1].Char == '\t')
                {
                    line.RemoveAt(pos.Col - 1);
                    change = true;
                }
                else
                {
                    var st = pos.Col - ctx.IndentSize;
                    st = st < 0 ? 0 : st;
                    line.RemoveRange(st, pos.Col - st);
                    change = true;
                }
            }

            return change;
        }

        internal static void Indent(IEditorContext ctx, Selection sel, IEnumerable<Character> indent)
        {
            var norm = sel.Normalize();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                var pos = HomeCommand.MoveHome(ctx.Buffer.Document, new Pos(i, line.Length));
                line.Insert(pos.Col, indent);
            }

            sel.Start = new Pos(sel.Start.Line, sel.Start.Col + indent.Count());
            sel.End = new Pos(sel.End.Line, sel.End.Col + indent.Count());
        }

        public override ICommand Clone()
        {
            return new TabCommand();
        }
    }
}
