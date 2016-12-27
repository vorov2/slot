using System;
using System.Windows.Forms;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.paste")]
    public sealed class PasteCommand : InsertRangeCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var str = Clipboard.GetText();
            
            if (Ed.HasBeforePaste)
            {
                var ev = new TextEventArgs(str);
                Ed.OnBeforePaste(ev);
                str = ev.Text;
            }

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
