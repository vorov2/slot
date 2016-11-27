using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.indent")]
    public sealed class IndentCommand : InsertRangeCommand
    {
        private List<int> undoIndents;
        private bool useTab;

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var startLine = Document.Lines[sel.Start.Line];

            if (sel.Start.Line != sel.End.Line)
            {
                redoSel = sel.Clone();
                useTab = Settings.UseTabs;
                undoIndents = Indent(View, sel);
                return Modify | Scroll;
            }
            else
            {
                var indent = View.UseTabs ? "\t"
                    : new string(' ', Line.GetIndentationSize(startLine.GetTetras(
                        sel.Start.Col, View.IndentSize), View.IndentSize));

                if (!View.UseTabs && startLine.WhiteSpaceBefore(sel.Start.Col))
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
                    spaces += View.IndentSize;
                else
                    return spaces;

            return spaces;
        }

        public override ActionResults Redo(out Pos pos)
        {
            if (undoIndents != null)
            {
                var norm = redoSel.Normalize();
                var c = norm.Start.Line;
                pos = redoSel.Caret;

                foreach (var i in undoIndents)
                {
                    var str = new string(useTab ? '\t' : ' ', i);
                    Document.Lines[c++].Insert(0, str.MakeCharacters());

                    if (c - 1 == pos.Line)
                        pos = new Pos(pos.Line, pos.Col + str.Length);
                }

                return Change;
            }
            else
                return base.Redo(out pos);
        }

        public override ActionResults Undo(out Pos pos)
        {
            if (undoIndents != null)
            {
                var norm = redoSel.Normalize();
                var c = norm.Start.Line;

                foreach (var i in undoIndents)
                    Document.Lines[c++].RemoveRange(0, i);

                pos = redoSel.Caret;
                return Change;
            }
            else
                return base.Undo(out pos);
        }

        internal static List<int> Indent(Editor ctx, Selection sel)
        {
            var norm = sel.Normalize();
            var undos = new List<int>();
            
            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                var indent = ctx.UseTabs ? "\t"
                    : new string(' ', Line.GetIndentationSize(line.GetTetras(
                        line.GetFirstNonIndentChar(), ctx.IndentSize), ctx.IndentSize));
                undos.Add(indent.Length);
                var pos = new Pos(i, 0);
                line.Insert(pos.Col, indent.MakeCharacters());

                if (i == norm.Start.Line)
                {
                    sel.Start = new Pos(sel.Start.Line, sel.Start.Col + indent.Length);
                    sel.End = new Pos(sel.End.Line, sel.End.Col + indent.Length);
                }
            }

            return undos;
        }

        internal override EditorCommand Clone()
        {
            return new IndentCommand();
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => false;
    }
}
