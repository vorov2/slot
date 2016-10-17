using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    internal sealed class CutCommand : DeleteRangeCommand
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            base.Execute(context, sel);
            var str = data.MakeString(context.Eol);
            Clipboard.SetText(str, TextDataFormat.UnicodeText);
        }
    }
}
