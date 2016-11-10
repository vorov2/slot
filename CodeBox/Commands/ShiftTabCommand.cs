using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | Scroll | Undoable)]
    public sealed class ShiftTabCommand : Command
    {
        private Selection redoSel;

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            redoSel = sel.Clone();
            TabCommand.Unindent(Context, sel);
            ShiftSel(sel);
            return ActionResults.Change;
        }

        public override Pos Redo()
        {
            var sel = redoSel;
            Execute(CommandArgument.Empty, sel);
            return sel.End;
        }

        public override Pos Undo()
        {
            var indent = Context.UseTabs ? "\t" : new string(' ', Context.TabSize);
            TabCommand.Indent(Context, redoSel, indent.MakeCharacters());
            ShiftSel(redoSel);
            return redoSel.Caret;
        }

        private void ShiftSel(Selection sel)
        {
            var indent = Context.UseTabs ? 1 : Context.TabSize;
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - indent);
            sel.End = new Pos(sel.End.Line, sel.End.Col - indent);
        }

        public override ICommand Clone()
        {
            return new ShiftTabCommand();
        }
    }
}
