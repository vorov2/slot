using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

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

        public override ActionResults Execute(Selection sel)
        {
            undoCaret = sel.Caret;
            var pos = togglePos == Pos.Empty ? Context.Caret : togglePos;
            var ln = pos.IsEmpty ? sel.Caret.Line : pos.Line;
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

            return Modify | ShallowChange;
        }

        public override ActionResults Undo(out Pos pos)
        {
            Context.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        public override ActionResults Redo(out Pos pos)
        {
            Context.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        public override IEditorCommand Clone()
        {
            return new ToggleFoldingCommand(togglePos);
        }

        public override bool SingleRun => true;
    }
}
