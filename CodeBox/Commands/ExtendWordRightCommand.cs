using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.extendwordright", "eswr")]
    public sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordRightCommand.WordRight(View, sel);

        internal override bool SupportLimitedMode => true;
    }
}
