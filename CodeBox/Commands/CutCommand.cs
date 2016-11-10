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
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public sealed class CutCommand : DeleteRangeCommand
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            var res = base.Execute(arg, sel);
            var str = data.MakeString(Buffer.Eol);
            Clipboard.SetText(str, TextDataFormat.UnicodeText);
            return res;
        }

        public override ICommand Clone()
        {
            return new CutCommand();
        }
    }
}
