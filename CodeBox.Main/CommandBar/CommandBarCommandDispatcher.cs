using System;
using System.ComponentModel.Composition;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;

namespace CodeBox.Main.CommandBar
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class CommandBarCommandDispatcher : CommandDispatcher
    {
        public const string Name = "commandbar";

        [Import]
        private IViewManager viewManager = null;

        [Command]
        public void ToggleCommandBar()
        {
            var ed = viewManager.GetActiveView() as Editor;

            if (ed != null)
            {
                var cm = CommandBarComponent.GetCommandBarControl();

                if (cm != null && cm.IsActive)
                    cm.CloseInput();
                else
                    cm.ShowInput();
            }
        }
    }
}
