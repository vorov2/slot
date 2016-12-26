using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.Settings
{
    public interface ISettingsManager : IComponent
    {
        ISettings Create();
    }
}
