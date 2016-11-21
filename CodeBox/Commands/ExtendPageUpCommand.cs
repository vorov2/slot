using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.extendpageup", "espu")]
    public sealed class ExtendPageUpCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageUpCommand.PageUp(View);
    }
}
