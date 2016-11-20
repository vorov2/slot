using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.documenthome")]
    public sealed class DocumentHomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => default(Pos);

        public override bool SupportLimitedMode => true;
    }
}
