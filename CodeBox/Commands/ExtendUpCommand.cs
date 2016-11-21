using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.extendup", "esu")]
    public sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => UpCommand.MoveUp(View, sel);
    }
}
