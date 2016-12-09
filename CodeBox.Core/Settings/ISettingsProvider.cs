using System;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Core.Settings
{
    public interface ISettingsProvider : IComponent
    {
        T Get<T>() where T : SettingsBag, new();

        void ReloadSettings(SettingsScope scope);
    }
}
