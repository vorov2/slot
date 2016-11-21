using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.extendhome", "esh")]
    public sealed class ExtendHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => HomeCommand.MoveHome(Document, sel.Caret);

        internal override bool SupportLimitedMode => true;
    }
}
