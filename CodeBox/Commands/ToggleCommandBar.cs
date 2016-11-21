using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Margins;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.commandbartoggle", "acb")]
    public sealed class ToggleCommandBar : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            var ed = View as Editor;

            if (ed != null)
            {
                if (ed.Parent is Editor)
                    ed = ed.Parent as Editor;

                var cm = ed.TopMargins.FirstOrDefault(b => b is CommandMargin) as CommandMargin;

                if (cm != null)
                    cm.Toggle();
            }

            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;

        internal override bool SupportLimitedMode => true;
    }
}
