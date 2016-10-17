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
    internal sealed class ShiftTabCommand : Command
    {
        private Selection redoSel;

        public override void Execute(EditorContext context, Selection sel)
        {
            redoSel = sel.Clone();
            TabCommand.Unindent(context, sel);
            ShiftSel(context, sel);
        }

        public override Pos Redo(EditorContext context)
        {
            var sel = redoSel;
            Execute(context, sel);
            return sel.End;
        }

        public override Pos Undo(EditorContext context)
        {
            var indent = context.Settings.UseTabs ? "\t" : new string(' ', context.Settings.TabSize);
            TabCommand.Indent(context, redoSel, indent.MakeCharacters());
            ShiftSel(context, redoSel);
            return redoSel.Caret;
        }

        private void ShiftSel(EditorContext context, Selection sel)
        {
            var indent = context.Settings.UseTabs ? 1 : context.Settings.TabSize;
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - indent);
            sel.End = new Pos(sel.End.Line, sel.End.Col - indent);
        }
    }
}
