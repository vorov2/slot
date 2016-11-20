using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.extendleft")]
    public sealed class ExtendLeftCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => LeftCommand.MoveLeft(Document, sel);

        public override bool SupportLimitedMode => true;
    }
}
