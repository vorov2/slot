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
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.commandbartoggle")]
    public sealed class ToggleCommandBar : EditorCommand
    {
        protected override ActionResults Execute(Selection sel)
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

        public override bool SingleRun => true;

        public override bool SupportLimitedMode => true;
    }
}
