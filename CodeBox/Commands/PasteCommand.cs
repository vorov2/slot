using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.paste")]
    public sealed class PasteCommand : InsertRangeCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var str = Clipboard.GetText();
            var ed = (Editor)View;
            
            if (ed.HasBeforePaste)
            {
                var ev = new TextEventArgs(str);
                ed.OnBeforePaste(ev);
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
