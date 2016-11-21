using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.extendright", "esr")]
    public sealed class ExtendRightCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => RightCommand.MoveRight(View, sel);

        public override bool SupportLimitedMode => true;
    }
}
