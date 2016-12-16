using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Json;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.Settings
{
    using MAP = Dictionary<string, object>;
    using OMAP = Dictionary<Type, SettingsBag>;

    [Export(typeof(ISettingsProvider))]
    [ComponentData("settings.default")]
    public sealed class SettingsProvider : ISettingsProvider
    {
        private const string FILE = "settings.json";

        [Import("directory.settings")]
        private string settingsDirectory = null;

        [Import("directory.user.settings")]
        private string userSettingsDirectory = null;

        [Import("directory.root")]
        private string rootDirectory = null;

        [Import]
        private IViewManager viewManager = null;

        private MAP settings;
        private MAP userSettings;
        private MAP workspaceSettings;
        private readonly OMAP bagMap = new OMAP();

        public T Get<T>() where T : SettingsBag, new()
        {
            LoadSettings();
            var typ = typeof(T);
            SettingsBag ret;

            if (!bagMap.TryGetValue(typ, out ret))
            {
                ret = new T();
                ret.Fill(settings, userSettings, workspaceSettings);
                bagMap.Add(typ, ret);
            }

            return (T)ret;
        }

        public void ReloadSettings(SettingsScope scope)
        {
            switch (scope)
            {
                case SettingsScope.Global:
                    settings = ReadFile(SettingsFile);
                    break;
                case SettingsScope.User:
                    userSettings = ReadFile(UserSettingsFile);
                    break;
                case SettingsScope.Workspace:
                    var dir = App.Catalog<IViewManager>().Default()?.GetActiveView()?.Workspace;
                    if (dir != null)
                        workspaceSettings = ReadFile(Path.Combine(dir.FullName, ".codebox", FILE));
                    break;
            }

            foreach (var b in bagMap.Values)
                b.Fill(settings, userSettings, workspaceSettings);
        }

        private void LoadSettings()
        {
            if (settings != null)
                return;

            settings = ReadFile(SettingsFile);
            userSettings = ReadFile(UserSettingsFile);

            var dir = viewManager.GetActiveView()?.Workspace;

            if (dir != null)
                workspaceSettings = ReadFile(Path.Combine(dir.FullName, ".codebox", FILE));
        }

        private MAP ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            var json = new JsonParser(File.ReadAllText(fileName));
            return json.Parse() as MAP;
        }

        private string SettingsFile => Path.Combine(rootDirectory, settingsDirectory, FILE);

        private string UserSettingsFile => Path.Combine(rootDirectory, userSettingsDirectory, FILE);
    }
}
