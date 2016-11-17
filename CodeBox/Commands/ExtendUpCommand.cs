using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.extendup")]
    public sealed class ExtendUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => UpCommand.MoveUp(Context, sel);
    }
}
