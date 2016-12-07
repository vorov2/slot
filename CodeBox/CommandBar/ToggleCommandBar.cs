using System;
using CodeBox.ObjectModel;
using CodeBox.Margins;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;
using CodeBox.CommandBar;
using CodeBox.Commands;

namespace CodeBox.CommandBar
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
                var cm = CommandBarComponent.GetCommandBarControl();

                if (cm != null && cm.IsActive)
                    cm.CloseInput();
                else
                    cm.ShowInput();
            }

            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}
