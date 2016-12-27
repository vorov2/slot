using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.Settings
{
    public interface ISettingsManager : IComponent
    {
        ISettings Create(IView view);
    }
}
