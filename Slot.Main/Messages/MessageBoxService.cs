using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.ComponentModel;
using Slot.Core.Messages;
using Slot.Core.ViewModel;

namespace Slot.Main.Messages
{
    [Export(typeof(IMessageBox))]
    [ComponentData(Name)]
    public sealed class MessageBoxService : IMessageBox
    {
        public const string Name = "messages.default";

        public MessageButtons Show(string caption, string text, MessageButtons buttons)
        {
            var par = Form.ActiveForm;
            var frm = new MessageWindow
            {
                Caption = caption ?? "",
                Detail = text ?? "",
                Buttons = buttons
            };
            frm.Height = frm.MeasureHeight();
            frm.MaximumSize = new Size(frm.Width, frm.Height);
            frm.MinimumSize = frm.MaximumSize;
            frm.ShowDialog(par);
            var vm = App.Component<IViewManager>().ActiveView;
            ((Control)vm.Editor).Focus();
            return frm.ButtonClicked;
        }
    }
}
