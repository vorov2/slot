using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Undoable | Scroll)]
    public sealed class PasteCommand : InsertRangeCommand
    {
        public override ActionChange Execute(CommandArgument arg, Selection sel)
        {
            var str = Clipboard.GetText();
            arg = new CommandArgument(str);
            return base.Execute(arg, sel);
        }
    }
}
