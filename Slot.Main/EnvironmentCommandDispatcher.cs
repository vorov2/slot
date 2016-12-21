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
using Slot.Core.Settings;
using Slot.Main;

namespace Slot.Test
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData("app")]
    public sealed class EnvironmentCommandDispatcher : CommandDispatcher
    {
        [Import]
        private ICommandBar commandBar = null;

        [Command]
        public void ToggleCommandBar()
        {
            var ed = ViewManager.GetActiveView() as EditorControl;

            if (ed != null)
            {
                var cm = CommandBarComponent.GetCommandBarControl();

                if (cm != null && cm.IsActive)
                    cm.CloseInput();
                else if (cm != null)
                    cm.ShowInput();
            }
        }

        [Command]
        public void ToggleMessage() => commandBar.ToggleMessage();

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

            App.Ext.Run(ViewManager.GetActiveView(), cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var set = App.Catalog<ISettingsProvider>().Default().Get<EnvironmentSettings>();
            set.Theme = themeName;
            set.UpdateSettings();
        }

        [Command]
        public void ChangeMode(string mode)
        {
            var mi = (Identifier)mode;

            if (mi != null && App.Ext.Grammars().GetGrammar(mi) != null)
                ViewManager.GetActiveView().Mode = mi;
            else
                App.Ext.Log($"Unknown mode: {mode}.", EntryType.Error);
        }
    }
}
