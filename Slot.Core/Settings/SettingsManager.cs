using System;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.Settings
{
    [Export(typeof(ISettingsManager))]
    [ComponentData(Name)]
    public sealed class SettingsManager : ISettingsManager
    {
        public const string Name = "settings.default";

        [Import("directory.user.settings")]
        private string userSettingsDirectory = null;

        public ISettings Create(IView view)
        {
            return new RealSettings(view) { UserSettingsDirectory = userSettingsDirectory };
        }
    }
}
