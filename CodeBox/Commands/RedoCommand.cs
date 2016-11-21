using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.redo", "eur")]
    public sealed class RedoCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            return Pure | NeedRedo | KeepRedo;
        }

        internal override bool SingleRun => true;
    }
}
