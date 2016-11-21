using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.selectline", "esl")]
    public sealed class SelectLineCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            var p = arg == null || !(arg is Pos) ? View.Caret : (Pos)arg;

            if (p.Line > -1)
            {
                sel.Start = new Pos(p.Line, 0);
                sel.End = new Pos(p.Line, Document.Lines[p.Line].Length);
            }

            return Clean;
        }
    }
}
