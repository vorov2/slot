using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.cut")]
    public sealed class CutCommand : DeleteRangeCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var res = base.Execute(sel);

            if (data != null)
            {
                var str = data.MakeString(Buffer.Eol);

                if (sel != Buffer.Selections[Buffer.Selections.Count - 1]
                    && Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    str = Clipboard.GetText(TextDataFormat.UnicodeText) +
                        Buffer.Eol.AsString() + str;
                }

                Clipboard.SetText(str, TextDataFormat.UnicodeText);
            }

            return res;
        }

        internal override EditorCommand Clone()
        {
            return new CutCommand();
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => true;
    }
}
