using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Output;
using Slot.Core.Packages;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Editor;
using Slot.Main.CommandBar;
using Slot.Main.File;

namespace Slot.Main
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

            if (!cm.Visible)
            {
                var act = ViewManager.GetActiveView();
                act = ViewManager.EnumerateViews()
                    .OrderByDescending(v => v.Buffer.LastAccess)
                    .FirstOrDefault(v => v != act);

                if (act != null)
                {
                    ViewManager.ActivateView(act);
                    ToggleCommandBar();
                }

                return;
            }

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

        public enum Info
        {
            [FieldName("about")]
            About,

            [FieldName("release")]
            Release,

            [FieldName("license")]
            License,

            [FieldName("changelog")]
            ChangeLog,

            [FieldName("packages")]
            Packages
        }

        [Command]
        public void ShowInfo(Info info)
        {
            switch (info)
            {
                case Info.About:
                    ShowAbout();
                    break;
                case Info.Release:
                    ShowInfoFile("readme.txt");
                    break;
                case Info.License:
                    ShowInfoFile("license.txt");
                    break;
                case Info.ChangeLog:
                    ShowInfoFile("changelog.txt");
                    break;
                case Info.Packages:
                    ShowPackages();
                    break;
            }
        }

        private void ShowPackages()
        {
            var sb = new StringBuilder();

            foreach (var pkg in App.Catalog<IPackageManager>().Default().EnumeratePackages())
            {
                sb.AppendLine($"{pkg.Name} ({pkg.Key})");
                sb.AppendLine(pkg.Copyright);
                sb.AppendLine($"Version: {pkg.Version}");
                sb.AppendLine($"{pkg.Description}");
                sb.AppendLine();
            }

            var buffer = App.Catalog<IBufferManager>().Default().CreateBuffer();
            buffer.Truncate(sb.ToString());
            buffer.ClearDirtyFlag();
            buffer.File = new FileInfo(Path.Combine(GetRootPackage().Directory.FullName, "docs", "packages"));
            buffer.Flags |= BufferDisplayFlags.HideCommandBar | BufferDisplayFlags.HideStatusBar;

            var view = ViewManager.CreateView();
            FileCommandDispatcher.OpenBuffer(buffer, view);
            ViewManager.ActivateView(view);
        }

        private PackageMetadata GetRootPackage()
        {
            var id = (Identifier)"slot";
            return App.Catalog<IPackageManager>().Default()
                .EnumeratePackages()
                .FirstOrDefault(p => p.Key == id);
        }

        private void ShowInfoFile(string fileName)
        {
            var pkg = GetRootPackage();

            if (pkg != null)
            {
                var view = ViewManager.CreateView();
                var buffer = App.Catalog<IBufferManager>().Default().CreateBuffer(
                    new FileInfo(Path.Combine(pkg.Directory.FullName, "docs", fileName)), Encoding.UTF8);
                buffer.Flags |= BufferDisplayFlags.HideCommandBar | BufferDisplayFlags.HideStatusBar;
                FileCommandDispatcher.OpenBuffer(buffer, view);
                ViewManager.ActivateView(view);
            }
        }

        private void ShowAbout()
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
