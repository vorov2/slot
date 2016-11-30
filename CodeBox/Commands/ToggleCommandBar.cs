using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Margins;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;
using CodeBox.CommandLine;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.togglecommandbar")]
    public sealed class ToggleCommandBar : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var ed = View as Editor;

            if (ed != null)
            {
                var cm = ed.TopMargins.FirstOrDefault(b => b is CommandMargin) as CommandMargin;

                if (cm != null && cm.IsActive)
                    cm.Close();
                else
                    cm.Show();
            }

            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
