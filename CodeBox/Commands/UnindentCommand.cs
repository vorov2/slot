using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.unindent")]
    public sealed class UnindentCommand : EditorCommand
    {
        private Selection redoSel;

        public override ActionResults Execute(Selection sel)
        {
            redoSel = sel.Clone();
            var change = IndentCommand.Unindent(Context, sel);

            if (change)
            {
                ShiftSel(sel);
                return Modify | Scroll;
            }
            else
                return Clean;
        }

        public override ActionResults Redo(out Pos pos)
        {
            var sel = redoSel;
            Execute(sel);
            pos = sel.End;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            var indent = Context.UseTabs ? "\t" : new string(' ', Context.IndentSize);
            IndentCommand.Indent(Context, redoSel, indent.MakeCharacters());
            ShiftSel(redoSel);
            pos = redoSel.Caret;
            return Change;
        }

        private void ShiftSel(Selection sel)
        {
            var indent = Context.UseTabs ? 1 : Context.IndentSize;
            sel.Start = new Pos(sel.Start.Line, sel.Start.Col - indent);
            sel.End = new Pos(sel.End.Line, sel.End.Col - indent);
        }

        public override IEditorCommand Clone()
        {
            return new UnindentCommand();
        }

        public override bool ModifyContent => true;
    }
}
