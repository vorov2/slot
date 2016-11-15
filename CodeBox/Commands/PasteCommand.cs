using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class PasteCommand : InsertRangeCommand, IModifyContent
    {
        public override ActionResults Execute(Selection sel)
        {
            var str = Clipboard.GetText();
            base.insertString = str.MakeCharacters();
            return base.Execute(sel);
        }

        public override ICommand Clone()
        {
            return new PasteCommand();
        }
    }
}
