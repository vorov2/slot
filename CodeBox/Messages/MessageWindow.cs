using CodeBox.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CodeBox.Styling;
using System.Windows.Forms;

namespace CodeBox.Messages
{
    public sealed class MessageWindow : Overlay
    {
        private readonly Editor editor;

        public MessageWindow(Editor editor)
        {
            this.editor = editor;
        }

        public void Show(string text, MessageWindowButtons buttons)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

        }

        public override Color BackgroundColor =>
            ((PopupStyle)editor.Theme.GetStyle(StandardStyle.Popup)).BackColor;

        public override Color BorderColor =>
            ((PopupStyle)editor.Theme.GetStyle(StandardStyle.Popup)).BorderColor;
    }

    public enum MessageWindowButtons
    {
        None = 0xFF,

        Yes = 0x01,

        No = 0x02,

        Cancel = 0x04,

        Close = 0x08
    }
}
