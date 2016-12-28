using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Messages;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Slot.Drawing;

namespace Slot.Main.Messages
{

    public sealed class MessageButton : Control
    {
        private bool mouse;
        private readonly MessageButtons button;
        private readonly bool cancelButton;

        public MessageButton(MessageButtons button, bool selected, bool cancelButton)
        {
            this.button = button;
            Selected = selected;
            this.cancelButton = cancelButton;
            Text = button.GetDisplayName();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            TabStop = true;

            using (var g = CreateGraphics())
                MeasureSize(g);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var env = App.Component<IViewManager>().ActiveView.Settings.Get<EnvironmentSettings>();
            var theme = App.Component<ITheme>();
            var style = theme.GetStyle(StandardStyle.Default);

            var pen = Selected ? style.ForeColor.ThickPen() : style.ForeColor.Pen();
            var rect = new RectangleF(0, 0, Width - pen.Size(), Height - pen.Size());
            g.FillRectangle(
                mouse
                    ? theme.GetStyle(StandardStyle.PopupHover).BackColor.Brush()
                    : theme.GetStyle(StandardStyle.Popup).BackColor.Brush(),
                ClientRectangle);
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            g.DrawString(Text, env.Font, style.ForeColor.Brush(), rect, TextFormats.CenteredAll);
        }

        private void MeasureSize(Graphics g)
        {
            var env = App.Component<IViewManager>().ActiveView.Settings.Get<EnvironmentSettings>();
            var size = g.MeasureString(Text, env.Font);
            Width = (int)Math.Round(size.Width * 1.5, MidpointRounding.AwayFromZero);
            Height = (int)Math.Round(size.Height * 1.5, MidpointRounding.AwayFromZero);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right)
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouse = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouse = false;
            Invalidate();
            OnButtonClick();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouse = true;
            Focus();
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouse = false;
            Invalidate();
        }

        public bool ProcessKeys(Keys keys)
        {
            if (keys == Keys.Enter && Selected
                || keys == Keys.Escape && cancelButton)
            {
                PerformClick();
                return true;
            }

            if (keys.HasFlag(Keys.Alt))
            {
                var amp = Keys.None;

                switch (button)
                {
                    case MessageButtons.Ok: amp = Keys.O; break;
                    case MessageButtons.Save: amp = Keys.S; break;
                    case MessageButtons.DontSave: amp = Keys.D; break;
                    case MessageButtons.Yes: amp = Keys.Y; break;
                    case MessageButtons.No: amp = Keys.N; break;
                    case MessageButtons.Close:
                    case MessageButtons.Cancel: amp = Keys.C; break;
                }

                keys &= ~Keys.Alt;

                if (keys == amp)
                {
                    PerformClick();
                    return true;
                }
            }

            return false;
        }

        public bool Selected { get; set; }

        public bool CancelButton => cancelButton;

        public void PerformClick() => OnButtonClick();

        public event EventHandler<MessageButtonEventArgs> ButtonClick;
        private void OnButtonClick() => ButtonClick?.Invoke(this, new MessageButtonEventArgs(button));
    }
}
