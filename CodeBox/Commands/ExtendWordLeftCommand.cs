using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.extendwordleft", "eswl")]
    public sealed class ExtendWordLeftCommandCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => WordLeftCommand.WordLeft(View, sel);

        public override bool SupportLimitedMode => true;
    }
}
