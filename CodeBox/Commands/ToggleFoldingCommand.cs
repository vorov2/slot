using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun | Undoable | Invalidate)]
    public sealed class ToggleFoldingCommand : Command
    {
        private int undoLine;
        private Pos undoCaret;

        public override bool Execute(CommandArgument arg, Selection sel)
        {
            undoLine = arg.Pos.Line;
            undoCaret = sel.Caret;
            Context.Folding.ToggleExpand(undoLine);
            return true;
        }

        public override Pos Undo()
        {
            Context.Folding.ToggleExpand(undoLine);
            return undoCaret;
        }

        public override Pos Redo()
        {
            Context.Folding.ToggleExpand(undoLine);
            return undoCaret;
        }
    }
}
