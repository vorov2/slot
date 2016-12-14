using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.CommandModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData(Name)]
    public sealed class GotoLineCommand : EditorCommand
    {
        public const string Name = "editor.gotoLine";

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var line = GetArg(0, args, 0) - 1;

            if (line < 0)
            {
                var alias = App.Catalog<ICommandProvider>().Default().GetCommandByKey(Cmd.GotoLine).Alias;
                App.Catalog<ICommandBar>().Default().Show(View, alias);
                return Pure;
            }

            if (line >= Document.Lines.Count) line = Document.Lines.Count - 1;

            var tl = Document.Lines[line];

            sel.Clear(new Pos(line, sel.Caret.Col > tl.Length ? tl.Length : sel.Caret.Col));
            return Scroll;
        }

        internal override bool SingleRun => true;
    }
}
