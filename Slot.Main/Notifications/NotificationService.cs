using System.ComponentModel.Composition;
using Slot.Core;
using Slot.Core.ComponentModel;
using Slot.Core.Notifications;
using Slot.Core.ViewModel;

namespace Slot.Main.Notifications
{
    [Export(typeof(INotificationService))]
    [ComponentData(Name)]
    public sealed class NotificationService : INotificationService
    {
        public const string Name = "notifications.default";

        public void ToggleNotification()
        {
            var hc = GetHeaderControl();

            if (hc != null)
                hc.ToggleNotification();
        }

        internal static HeaderControl GetHeaderControl()
        {
            return (HeaderControl)App.Component<IViewManager>().ActiveView.Header;
        }
    }
}
