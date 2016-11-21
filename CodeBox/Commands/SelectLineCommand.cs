using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.selectline", "esl")]
    public sealed class SelectLineCommand : EditorCommand
    {
        private Pos pos;

        public SelectLineCommand(Pos pos)
        {
            this.pos = pos;
        }

        public SelectLineCommand()
        {
            pos = Pos.Empty;
        }

        protected override ActionResults Execute(Selection sel)
        {
            var p = pos == Pos.Empty ? View.Caret : pos;

            if (p.Line > -1)
            {
                sel.Start = new Pos(p.Line, 0);
                sel.End = new Pos(p.Line, Document.Lines[p.Line].Length);
            }

            return Clean;
        }
    }
}
