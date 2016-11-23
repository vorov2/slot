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
            var p = args == null || args.Length == 0 || !(args[0] is int) ? View.Caret : new Pos((int)args[0], 0);

            if (p.Line > -1)
            {
                sel.Start = new Pos(p.Line, 0);
                sel.End = new Pos(p.Line, Document.Lines[p.Line].Length);
            }

            return Clean;
        }
    }
}
