using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.Notifications
{
    public interface INotificationService : IComponent
    {
        void ToggleNotification();
    }
}
