using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.RestoreCaret | ActionExponent.Undoable | ActionExponent.Scroll)]
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
