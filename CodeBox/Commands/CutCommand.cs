using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.cut")]
    public sealed class CutCommand : DeleteRangeCommand
    {
        protected override ActionResults Execute(Selection sel)
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

        public override bool ModifyContent => true;

        public override bool SupportLimitedMode => true;
    }
}
