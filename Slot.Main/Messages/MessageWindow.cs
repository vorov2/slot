using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
    public partial class MessageWindow : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        public MessageWindow()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
            ShowInTaskbar = ControlBox = MinimizeBox = MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(Dpi.GetWidth(3), Dpi.GetHeight(10), Dpi.GetWidth(3), Dpi.GetWidth(3));
            Width = Dpi.GetWidth(420);
            KeyPreview = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var theme = App.Component<ITheme>();
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(theme.GetStyle(StandardStyle.PopupBorder).ForeColor.Brush(), ClientRectangle);
            g.FillRectangle(theme.GetStyle(StandardStyle.Popup).BackColor.Brush(),
                new Rectangle(ClientRectangle.X + Padding.Left, ClientRectangle.Y + Padding.Top,
                    ClientRectangle.Width - Padding.Right - Padding.Left,
                    ClientRectangle.Height - Padding.Top - Padding.Bottom));

            var env = App.Component<IViewManager>().ActiveView.Settings.Get<EnvironmentSettings>();
            var padx = Dpi.GetWidth(30);
            var pady = Dpi.GetWidth(20);

            using (var bigfont = new Font(env.FontName, env.FontSize + 3))
            {
                var x = Padding.Left + padx;
                var y = Padding.Top + pady + 0f;
                var width = Width - Padding.Left - Padding.Right - padx * 2;
                var size = g.MeasureString(Caption, bigfont, width, TextFormats.Wrap);

                g.DrawString(Caption, bigfont, theme.GetStyle(StandardStyle.Keyword).ForeColor.Brush(), 
                    new RectangleF(x, y, width, Height), TextFormats.Wrap);
                y += size.Height + bigfont.Height;

                foreach (var str in Detail.Split('\n'))
                {
                    size = g.MeasureString(str, env.Font, width, TextFormats.Wrap);
                    g.DrawString(str, env.Font, theme.GetStyle(StandardStyle.Default).ForeColor.Brush(),
                        new RectangleF(x, y, width, Height), TextFormats.Wrap);
                    y += size.Height;
                }

                y += bigfont.Height;
                AddButtons(x, (int)y);
            }

            base.OnPaint(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right)
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Left || e.KeyData == Keys.Right)
            {
                var inc = e.KeyData == Keys.Left ? -1 : 1;
                var arr = Controls.OfType<MessageButton>().ToList();
                var bt = arr.FirstOrDefault(b => b.Selected);
                var idx = 0;

                if (bt != null)
                {
                    bt.Selected = false;
                    idx = arr.IndexOf(bt) + inc;

                    if (idx >= arr.Count)
                        idx = 0;
                    else if (idx < 0)
                        idx = arr.Count - 1;
                }

                ((MessageButton)Controls[idx]).Selected = true;
                Invalidate(true);
            }
            else
            {
                foreach (var b in Controls.OfType<MessageButton>())
                    if (b.ProcessKeys(e.KeyData))
                        break;
            }
        }

        private bool buttonsAdded;
        private void AddButtons(int x, int y)
        {
            if (buttonsAdded)
                return;

            if (Buttons.HasFlag(MessageButtons.Ok))
                x += AddButton(x, y, MessageButtons.Ok, true);
            if (Buttons.HasFlag(MessageButtons.Yes))
                x += AddButton(x, y, MessageButtons.Yes);
            if (Buttons.HasFlag(MessageButtons.No))
                x += AddButton(x, y, MessageButtons.No, true);
            if (Buttons.HasFlag(MessageButtons.Save))
                x += AddButton(x, y, MessageButtons.Save, true);
            if (Buttons.HasFlag(MessageButtons.SaveAll))
                x += AddButton(x, y, MessageButtons.SaveAll, true);
            if (Buttons.HasFlag(MessageButtons.DontSave))
                x += AddButton(x, y, MessageButtons.DontSave);
            if (Buttons.HasFlag(MessageButtons.Cancel))
                x += AddButton(x, y, MessageButtons.Cancel, false, true);
            if (Buttons.HasFlag(MessageButtons.Close))
                x += AddButton(x, y, MessageButtons.Close, true, true);

            if (Controls.Count > 0)
                Controls[0].Focus();

            buttonsAdded = true;
        }

        private int AddButton(int x, int y, MessageButtons button, bool selected = false, bool cancel = false)
        {
            var but = new MessageButton(button, selected, cancel)
            {
                Top = y,
                Left = x
            };
            but.ButtonClick += (o, e) =>
            {
                ButtonClicked = e.Button;
                AllowClose = true;
                Close();
            };
            Controls.Add(but);
            but.Invalidate();
            return but.Width + Dpi.GetWidth(10);
        }

        internal int MeasureHeight()
        {
            var env = App.Component<IViewManager>().ActiveView.Settings.Get<EnvironmentSettings>();
            var padx = Dpi.GetWidth(30);
            var pady = Dpi.GetWidth(20);

            using (var g = CreateGraphics())
            using (var bigfont = new Font(env.FontName, env.FontSize + 3))
            {
                var x = Padding.Left + padx;
                var y = Padding.Top + pady + 0f;
                var width = Width - Padding.Left - Padding.Right - padx * 2;
                var size = g.MeasureString(Caption, bigfont, width, TextFormats.Wrap);
                y += size.Height + bigfont.Height;

                foreach (var str in Detail.Split('\n'))
                {
                    size = g.MeasureString(str, env.Font, width, TextFormats.Wrap);
                    y += size.Height;
                }

                var ret = (int)Math.Round(y + pady, MidpointRounding.AwayFromZero);

                if (Buttons != MessageButtons.None)
                {
                    ret += bigfont.Height;
                    ret += env.Font.Height * 2;
                }

                return ret;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!AllowClose)
                e.Cancel = true;
        }

        private bool AllowClose { get; set; }

        public string Caption { get; set; }

        public string Detail { get; set; }

        public MessageButtons Buttons { get; set; }

        public MessageButtons ButtonClicked { get; private set; }
    }
}
