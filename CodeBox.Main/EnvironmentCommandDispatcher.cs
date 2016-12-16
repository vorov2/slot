using Slot.ComponentModel;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Keyboard;
using Slot.Core.Output;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Main.CommandBar;
using Slot.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Test
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData("app")]
    public sealed class EnvironmentCommandDispatcher : CommandDispatcher
    {
        [Import]
        private IViewManager viewManager = null;

        [Command]
        public void ToggleCommandBar()
        {
            var ed = viewManager.GetActiveView() as EditorControl;

            if (ed != null)
            {
                var cm = CommandBarComponent.GetCommandBarControl();

                if (cm != null && cm.IsActive)
                    cm.CloseInput();
                else
                    cm.ShowInput();
            }
        }

        [Command]
        public void CommandPalette(string commandName)
        {
            var cmd = App.Catalog<ICommandProvider>().Default().EnumerateCommands()
                .FirstOrDefault(c => c.Title.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (cmd == null)
            {
                App.Ext.Log($"Invalid command name: {commandName}.", EntryType.Error);
                return;
            }

            App.Ext.Run(viewManager.GetActiveView(), cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var theme = App.Catalog<IThemeComponent>().Default();
            theme.ChangeTheme((Identifier)themeName);
        }

        [Command]
        public void ChangeMode(string mode)
        {
            if (mode != null && App.Ext.Grammars().GetGrammar(mode) != null)
                viewManager.GetActiveView().Mode = mode;
            else
                App.Ext.Log($"Unknown mode: {mode}.", EntryType.Error);
        }
    }
}
