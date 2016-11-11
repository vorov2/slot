using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public sealed class ToggleFoldingCommand : Command, IModifyContent
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

            return SingleRun | Modify | ShallowChange;
        }

        public override ActionResults Undo(out Pos pos)
        {
            Context.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return SingleRun | Modify | ShallowChange;
        }

        public override ActionResults Redo(out Pos pos)
        {
            Context.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return SingleRun | Modify | ShallowChange;
        }

        public override ICommand Clone()
        {
            return new ToggleFoldingCommand();
        }
    }
}
