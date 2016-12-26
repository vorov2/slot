using System;

namespace Slot.Core.Settings
{
    public interface ISettings
    {
        T Get<T>() where T : SettingsBag, new();

        void ReloadSettings(SettingsScope scope);
    }
}
