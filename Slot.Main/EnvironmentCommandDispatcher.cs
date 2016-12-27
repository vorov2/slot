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
using System.Reflection;
using Slot.Core.Packages;
using Slot.Main.File;

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
            var cm = CommandBarComponent.GetCommandBarControl();

            if (cm != null && cm.IsActive)
                cm.CloseInput();
            else if (cm != null)
                cm.ShowInput();
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

            App.Ext.Run(cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            App.Catalog<ITheme>().Default().ChangeTheme((Identifier)themeName);
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

        [Command]
        public void Exit()
        {
            Application.Exit();
        }

        [Command]
        public void ShowReleaseNotes()
        {
            var id = (Identifier)"slot";
            var pkg = App.Catalog<IPackageManager>().Default()
                .EnumeratePackages()
                .FirstOrDefault(p => p.Key == id);

            if (pkg != null)
            {
                ViewManager.CreateView();
                var buffer = App.Catalog<IBufferManager>().Default().CreateBuffer(
                    new FileInfo(Path.Combine(pkg.Directory.FullName, "docs", "readme.txt")), Encoding.UTF8);
                FileCommandDispatcher.OpenBuffer(buffer);
            }
        }

        [Command]
        public void ShowVersion()
        {
            var asm = typeof(EnvironmentCommandDispatcher).Assembly;
            var v = asm.GetName().Version;
            var attr = Attribute.GetCustomAttribute(asm, typeof(AssemblyConfigurationAttribute))
                as AssemblyConfigurationAttribute;
            var build = attr.Configuration == "Insiders" ? v.Revision + "-Insiders":"Public";
            var plat = App.IsMono ? "Mono" : ".NET";
            MessageBox.Show(Form.ActiveForm,
                $"{Application.ProductName}\n\nVersion: {v.Major}.{v.Minor}.{v.Build}\nBuild: {build}\nDate: {DateTime.Parse(App.BuildDate).ToString("dd/MM/yyyy")}\nRuntime: {plat}_{Environment.Version}",
                Application.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
