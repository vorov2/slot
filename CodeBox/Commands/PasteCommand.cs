using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.paste")]
    public sealed class PasteCommand : InsertRangeCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            var str = Clipboard.GetText();
            base.insertString = str.MakeCharacters();
            return base.Execute(sel);
        }

        public override IEditorCommand Clone()
        {
            return new PasteCommand();
        }
    }
}
