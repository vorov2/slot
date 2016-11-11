using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public sealed class PasteCommand : InsertRangeCommand
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var str = Clipboard.GetText();
            arg = new CommandArgument(str);
            return base.Execute(arg, sel);
        }

        public override ICommand Clone()
        {
            return new PasteCommand();
        }
    }
}
