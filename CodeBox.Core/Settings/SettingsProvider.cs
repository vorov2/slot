using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Json;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Core.Settings
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
                    settings = ReadFile(SettingsDirectory);
                    break;
                case SettingsScope.User:
                    userSettings = ReadFile(UserSettingsDirectory);
                    break;
                case SettingsScope.Workspace:
                    break;
            }

            foreach (var b in bagMap.Values)
                b.Fill(settings, userSettings, workspaceSettings);
        }

        private void LoadSettings()
        {
            if (settings != null)
                return;

            settings = ReadFile(SettingsDirectory);
            userSettings = ReadFile(UserSettingsDirectory);
        }

        private MAP ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            var json = new JsonParser(File.ReadAllText(fileName));
            return json.Parse() as MAP;
        }

        private string SettingsDirectory => Path.Combine(rootDirectory, settingsDirectory, FILE);

        private string UserSettingsDirectory => Path.Combine(rootDirectory, userSettingsDirectory, FILE);
    }
}
