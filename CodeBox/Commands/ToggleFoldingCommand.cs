using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;
using CodeBox.Folding;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun | Undoable | Invalidate)]
    public sealed class ToggleFoldingCommand : Command
    {
        private int undoLine;
        private Pos undoCaret;

        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            undoCaret = sel.Caret;
            var ln = arg.Pos.IsEmpty ? sel.Caret.Line : arg.Pos.Line;
            var level = -1;

            while (ln > -1)
            {
                var line = Document.Lines[ln];

                if (line.Folding.Has(FoldingStates.Header) && (level == -1 || line.FoldingLevel < level))
                {
                    undoLine = ln;
                    Context.Folding.ToggleExpand(undoLine);
                    break;
                }

                if (level == -1)
                    level = line.FoldingLevel;

                ln--;
            }

            return ActionResults.Clean;
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

        public override ICommand Clone()
        {
            return new ToggleFoldingCommand();
        }
    }
}
