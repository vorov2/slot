using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.RestoreCaret | ActionExponent.Scroll | ActionExponent.Undoable)]
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
