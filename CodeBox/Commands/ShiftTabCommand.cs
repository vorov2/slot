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

        public override void Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();
            TabCommand.Unindent(Context, sel);
            ShiftSel(sel);
        }

        public override Pos Redo()
        {
            var sel = redoSel;
            Execute(default(CommandArgument), sel);
            return sel.End;
        }

        public override Pos Undo()
        {
            var indent = Settings.UseTabs ? "\t" : new string(' ', Settings.TabSize);
            TabCommand.Indent(Context, redoSel, indent.MakeCharacters());
            ShiftSel(redoSel);
            return redoSel.Caret;
        }

        private void ShiftSel(Selection sel)
        {
            var indent = Settings.UseTabs ? 1 : Settings.TabSize;
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - indent);
            sel.End = new Pos(sel.End.Line, sel.End.Col - indent);
        }
    }
}
