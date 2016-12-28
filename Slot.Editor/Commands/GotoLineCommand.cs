using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ViewModel;

namespace Slot.Editor.Commands
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
                var alias = App.Component<ICommandProvider>().GetCommandByKey(Cmd.GotoLine).Alias;
                App.Component<ICommandBar>().Show(alias);
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
