using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.paste", "ebv")]
    public sealed class PasteCommand : InsertRangeCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            var str = Clipboard.GetText();
            base.insertString = str.MakeCharacters();
            return base.Execute(sel);
        }

        internal override EditorCommand Clone()
        {
            return new PasteCommand();
        }

        internal override bool SupportLimitedMode => true;
    }
}
