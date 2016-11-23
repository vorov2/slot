using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.undo")]
    public sealed class UndoCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            return Pure | NeedUndo | KeepRedo;
        }

        internal override bool SingleRun => true;
    }
}
