using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.extenddown")]
    public sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => DownCommand.MoveDown(Context, sel);
    }
}
