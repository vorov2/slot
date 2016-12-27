using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Slot.Core.ViewModel;
using Slot.Editor;
using Slot.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Slot.Core.Themes;
using System.Drawing;
using Slot.Core;
using Slot.Core.Output;

namespace Slot.Main.CommandBar
{
    public sealed class HeaderControl : Control
    {
        private readonly EditorControl editor;
        private string error;
        private Rectangle errorButton;
        private MessageOverlay overlay;

        public HeaderControl(EditorControl editor)
        {
            this.editor = editor;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Cursor = Cursors.Default;
            Dock = DockStyle.Top;
            editor.Escape += EditorEscape;
            App.Catalog<ILogComponent>().Default().EntryWritten += (o, e) =>
            {
                if (overlay != null && overlay.Visible)
                    HideTip();

                if (e.Type == EntryType.Error)
                    error = e.Data;
                else
                    error = null;

                Invalidate();
            };
        }

        internal void AdjustHeight()
        {
            var frm = FindForm();
            var h = (int)Math.Round(
                Math.Max(((IView)frm).Settings.Get<EnvironmentSettings>().Font.Height() * 1.7,
                    editor.Info.CharHeight * 1.9), MidpointRounding.AwayFromZero);
            if (Height != h)
                Height = h;
        }

        private void EditorEscape(object sender, EventArgs e)
        {
            var ovl = GetMessageOverlay();

            if (ovl != null && ovl.Visible)
                HideTip();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AdjustHeight();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustHeight();

            if (ClientSize.Width != lastTipClientWidth)
                HideTip();

            var g = e.Graphics;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var bounds = ClientRectangle;
            var cs = editor.Theme.GetStyle(StandardStyle.CommandBar);
            var acs = editor.Theme.GetStyle(StandardStyle.CommandBarCaption);
            g.FillRectangle(cs.BackColor.Brush(), bounds);

            var baseFont = ((IView)FindForm()).Settings.Get<EnvironmentSettings>().Font;
            var font = baseFont.Get(cs.FontStyle);
            var x = font.Width() * 2f;
            var h = font.Width() / 1.8f;
            var y = bounds.Y + ((bounds.Height - h) / 2);
            var w = h;

            if (!editor.ReadOnly)
            {
                if (editor.Buffer.IsDirty)
                    g.FillEllipse(cs.ForeColor.Brush(), x, y, w, h);
                else
                    g.DrawEllipse(cs.ForeColor.Pen(), x, y, w, h);

                x += font.Width();
            }

            y = (bounds.Height - font.Height()) / 2;
            g.DrawString(editor.Buffer.File.Name, font, cs.ForeColor.Brush(), x, y, TextFormats.Compact);
            x += g.MeasureString(editor.Buffer.File.Name, font).Width;

            var ws = ((IView)FindForm()).Workspace?.FullName.Length ?? 0;
            var dirName = editor.Buffer.File.DirectoryName.Length < ws ? editor.Buffer.File.DirectoryName
                : editor.Buffer.File.DirectoryName.Substring(ws).TrimStart('/', '\\');
            g.DrawString(dirName, font.Get(acs.FontStyle), acs.ForeColor.Brush(),
                new RectangleF(x, y, bounds.Width - x - font.Width(), bounds.Height), TextFormats.Path);

            var tipRect = new Rectangle(bounds.Width - font.Width() * 2,
                bounds.Y + ((bounds.Height - font.Width()) / 2), font.Width(), font.Width());

            if (error != null)
            {
                errorButton = tipRect;
                g.FillRectangle(editor.Theme.GetStyle(StandardStyle.Error).ForeColor.Brush(), errorButton);
            }
            else
            {
                errorButton = default(Rectangle);
                g.FillRectangle(editor.Theme.GetStyle(StandardStyle.Default).BackColor.Brush(), tipRect);
                g.DrawRectangle(ControlPaint.Dark(cs.BackColor, .2f).Pen(), tipRect);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var loc = e.Location;

            if (loc.X >= errorButton.Left && loc.X <= errorButton.Right
                && loc.Y >= errorButton.Top && loc.Y <= errorButton.Bottom)
                ToggleTip();
            else
                App.Ext.Run(Cmd.OpenFile);

            Invalidate();
            base.OnMouseDown(e);
        }

        private MessageOverlay GetMessageOverlay()
        {
            if (overlay == null)
            {
                overlay = new MessageOverlay(editor);
                overlay.Visible = false;
                overlay.Click += (_, __) => HideTip();
                FindForm().Controls.Add(overlay);
                overlay.BringToFront();
            }

            return overlay;
        }
        
        public void ToggleTip()
        {
            var wnd = GetMessageOverlay();
            if (wnd == null || !wnd.Visible)
                ShowTip();
        }

        private int lastTipClientWidth;
        private void ShowTip()
        {
            lastTipClientWidth = ClientSize.Width;
            var font = ((IView)FindForm()).Settings.Get<EnvironmentSettings>().Font;
            var eWidth = editor.Info.TextWidth / 2;
            Size size;
            var err = "M " + error;
            error = null;

            using (var g = CreateGraphics())
                size = g.MeasureString(err, font, eWidth).ToSize();

            var ovl = GetMessageOverlay();
            var xpad = Dpi.GetWidth(8);
            var ypad = Dpi.GetHeight(4);
            ovl.Padding = new Padding(xpad, ypad, xpad, ypad);
            ovl.Width = size.Width + xpad * 2 + ovl.BorderWidth * 2;
            ovl.Height = size.Height + ypad * 2 + ovl.BorderWidth * 2;
            ovl.Location = new Point(FindForm().ClientRectangle.Width - ovl.Width, ClientSize.Height + editor.Info.TextTop);
            ovl.Font = font;
            ovl.Text = err;
            ovl.Visible = true;
        }

        private void HideTip()
        {
            var ovl = GetMessageOverlay();
            if (ovl != null && ovl.Visible)
            {
                ovl.Visible = false;
                error = null;
            }
        }
    }

}

