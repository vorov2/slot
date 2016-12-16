using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.Settings
{
    public interface ISettingsProvider : IComponent
    {
        T Get<T>() where T : SettingsBag, new();

        void ReloadSettings(SettingsScope scope);
    }
}
