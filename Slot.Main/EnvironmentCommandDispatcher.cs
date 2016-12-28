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
using Slot.Core.Notifications;
using Slot.Core.Output;
using Slot.Core.Packages;
using Slot.Core.Settings;
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
        private INotificationService notifications = null;

        [Import("directory.user.settings")]
        private string userSettingsDirectory = null;

        [Command]
        public void ToggleCommandBar()
        {
            var cb = App.Component<ICommandBar>();

            if (cb.InputVisible)
                cb.Hide();
            else
                cb.Show();
        }

        [Command]
        public void ToggleNotification() => notifications.ToggleNotification();

        [Command]
        public void CommandPalette(string commandName)
        {
            var cmd = App.Component<ICommandProvider>().EnumerateCommands()
                .FirstOrDefault(c => c.Title != null && c.Title.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (cmd == null)
            {
                App.Ext.Log($"Invalid command name: {commandName}.", EntryType.Error);
                return;
            }

            App.Component<ICommandBar>().Hide();
            App.Ext.Run(cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            App.Component<ITheme>().ChangeTheme((Identifier)themeName);
        }

        [Command]
        public void ChangeMode(string mode)
        {
            var mi = (Identifier)mode;

            if (mi != null && App.Ext.Grammars().GetGrammar(mi) != null)
                ViewManager.ActiveView.Mode = mi;
            else
                App.Ext.Log($"Unknown mode: {mode}.", EntryType.Error);
        }

        [Command]
        public void Exit()
        {
            App.Close();
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

        [Command]
        public void OpenSettings(string settings)
        {
            if (settings.Equals("global", StringComparison.OrdinalIgnoreCase))
                ShowGlobalSettings();
            else if (settings.Equals("user", StringComparison.OrdinalIgnoreCase))
                ShowSettings(userSettingsDirectory);
            else if (settings.Equals("workspace", StringComparison.OrdinalIgnoreCase))
            {
                var ws = ViewManager.ActiveView.Workspace;
                ShowSettings(Path.Combine(ws.FullName, ".slot"));
            }
        }

        private void ShowSettings(string dir)
        {
            var fi = new FileInfo(Path.Combine(dir, "settings.json"));

            if (!fi.Exists)
            {
                if (!FileUtil.EnsureFilePath(fi))
                    return;

                using (var sr = new StreamWriter(fi.OpenWrite()))
                {
                    sr.WriteLine("//Place your settings here");
                    sr.WriteLine("{");
                    sr.WriteLine("}");
                }
            }

            var buffer = App.Component<IBufferManager>().CreateBuffer(fi, Encoding.UTF8);
            OpenBuffer(buffer);
        }

        private void ShowGlobalSettings()
        {
            var str = App.Component<ISettingsManager>().GenerateGlobalSettings();
            var buffer = App.Component<IBufferManager>().CreateBuffer();
            str = "//You can override settings from this file in user settings\n//or workspace settings.\n" + str;
            buffer.Truncate(str);
            buffer.File = new FileInfo(Path.Combine(GetRootPackage().Directory.FullName, "docs", "settings.json"));
            OpenSpecialBuffer(buffer);
        }

        private void ShowPackages()
        {
            var sb = new StringBuilder();

            foreach (var pkg in App.Component<IPackageManager>().EnumeratePackages())
            {
                sb.AppendLine($"{pkg.Name} ({pkg.Key})");
                sb.AppendLine(pkg.Copyright);
                sb.AppendLine($"Version: {pkg.Version}");
                sb.AppendLine($"{pkg.Description}");
                sb.AppendLine();
            }

            var buffer = App.Component<IBufferManager>().CreateBuffer();
            buffer.Truncate(sb.ToString());
            buffer.File = new FileInfo(Path.Combine(GetRootPackage().Directory.FullName, "docs", "packages"));
            OpenSpecialBuffer(buffer);
        }

        private PackageMetadata GetRootPackage()
        {
            var id = (Identifier)"slot";
            return App.Component<IPackageManager>()
                .EnumeratePackages()
                .FirstOrDefault(p => p.Key == id);
        }

        private void ShowInfoFile(string fileName)
        {
            var pkg = GetRootPackage();

            if (pkg != null)
            {
                var buffer = App.Component<IBufferManager>().CreateBuffer(
                    new FileInfo(Path.Combine(pkg.Directory.FullName, "docs", fileName)), Encoding.UTF8);
                OpenSpecialBuffer(buffer);
            }
        }

        private void OpenSpecialBuffer(IBuffer buffer)
        {
            buffer.Flags |= BufferDisplayFlags.HideHeader | BufferDisplayFlags.HideStatusBar
                | BufferDisplayFlags.HideWorkspace | BufferDisplayFlags.ReadOnly;
            OpenBuffer(buffer);
        }

        private void OpenBuffer(IBuffer buffer)
        {
            var view = ViewManager.CreateView();
            FileCommandDispatcher.OpenBuffer(buffer, view);
            ViewManager.ActivateView(view);
        }

        private void ShowAbout()
        {
            var asm = typeof(EnvironmentCommandDispatcher).Assembly;
            var v = asm.GetName().Version;
            var attr = Attribute.GetCustomAttribute(asm, typeof(AssemblyConfigurationAttribute))
                as AssemblyConfigurationAttribute;
            var build = $"{v.Revision}-{attr.Configuration}";
            var plat = App.IsMono ? "Mono" : ".NET";
            App.Ext.Show(Application.ProductName,
                $"Version: {v.Major}.{v.Minor}.{v.Build}\nBuild: {build}\nCommit: {App.Commit}\nDate: {DateTime.Parse(App.BuildDate).ToString("dd/MM/yyyy")}\nRuntime: {plat} {Environment.Version}");
        }
    }
}
