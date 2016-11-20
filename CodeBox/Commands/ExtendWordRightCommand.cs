using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.extendwordright")]
    public sealed class ExtendWordRightCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordRightCommand.WordRight(View, sel);

        public override bool SupportLimitedMode => true;
    }
}
