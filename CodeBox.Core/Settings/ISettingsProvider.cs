using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.Settings
{
    public interface ISettingsProvider : IComponent
    {
        T Get<T>() where T : SettingsBag, new();

        void ReloadSettings();
    }
}
