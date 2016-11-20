using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.foldingtoggle")]
    public sealed class ToggleFoldingCommand : EditorCommand
    {
        private int undoLine;
        private Pos undoCaret;
        private Pos togglePos;

        public ToggleFoldingCommand(Pos togglePos)
        {
            this.togglePos = togglePos;
        }

        public ToggleFoldingCommand()
        {
            togglePos = Pos.Empty;
        }

        protected override ActionResults Execute(Selection sel)
        {
            undoCaret = sel.Caret;
            var pos = togglePos == Pos.Empty ? View.Caret : togglePos;
            var ln = pos.IsEmpty ? sel.Caret.Line : pos.Line;
            var level = -1;

            while (ln > -1)
            {
                var line = Document.Lines[ln];

                if (line.Folding.Has(FoldingStates.Header) && (level == -1 || line.FoldingLevel < level))
                {
                    undoLine = ln;
                    View.Folding.ToggleExpand(undoLine);
                    break;
                }

                if (level == -1)
                    level = line.FoldingLevel;

                ln--;
            }

            return Modify | ShallowChange;
        }

        public override ActionResults Undo(out Pos pos)
        {
            View.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        public override ActionResults Redo(out Pos pos)
        {
            View.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        internal override EditorCommand Clone()
        {
            return new ToggleFoldingCommand(togglePos);
        }

        public override bool SingleRun => true;
    }
}
