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
    [CommandBehavior(Modify | RestoreCaret | Undoable | Scroll)]
    internal sealed class PasteCommand : InsertRangeCommand
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var str = Clipboard.GetText();
            context.String = str;
            base.Execute(context, sel);
        }
    }
}
