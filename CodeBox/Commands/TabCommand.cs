using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | Scroll | Undoable)]
    internal sealed class TabCommand : InsertRangeCommand
    {
        private bool undoIndent;

        public override void Execute(CommandArgument arg, Selection sel)
        {
            var indent = Settings.UseTabs ? "\t" : new string(' ', Settings.TabSize);
            
            if (sel.Start.Line != sel.End.Line)
            {
                undoIndent = true;
                redoSel = sel.Clone();
                var str = indent.MakeCharacters();
                Indent(Context, sel, str);
            }
            else
            {
                arg = new CommandArgument('\0', indent);
                base.Execute(arg, sel);
            }
        }

        public override Pos Redo()
        {
            if (undoIndent)
            {
                var sel = redoSel;
                Execute(default(CommandArgument), sel);
                return sel.End;
            }
            return base.Redo();
        }

        public override Pos Undo()
        {
            if (undoIndent)
            {
                Unindent(Context, redoSel);
                return redoSel.Caret;
            }
            else
                return base.Undo();
        }

        internal static void Unindent(IEditorContext ctx, Selection sel)
        {
            var norm = sel.Normalize();

            for (var i = norm.Start.Line; i < norm.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                var pos = HomeCommand.MoveHome(ctx.Buffer.Document, new Pos(i, line.Length));

                if (pos.Col == 0)
                    continue;
                else if (line[pos.Col - 1].Char == '\t')
                    line.RemoveAt(pos.Col - 1);
                else
                {
                    var st = pos.Col - ctx.Settings.TabSize;
                    st = st < 0 ? 0 : st;
                    line.RemoveRange(st, pos.Col - st);
                }
            }
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
    }
}
