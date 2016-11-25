using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.selectline")]
    public sealed class SelectLineCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var line = GetArg<int>(0, args, sel.Caret.Line + 1) - 1;

            if (line > -1)
            {
                sel.Start = new Pos(line, 0);
                sel.End = new Pos(line, Document.Lines[line].Length);
            }

            return Clean;
        }

        internal override bool SingleRun => true;
    }
}
