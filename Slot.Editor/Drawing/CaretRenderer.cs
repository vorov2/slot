using Slot.Core.Themes;
using System;
using System.Drawing;
using System.Windows.Forms;
using Slot.Drawing;

namespace Slot.Editor.Drawing
{
    internal sealed class CaretRenderer : IDisposable
    {
        private const int INTERVAL = 500;
        
        private readonly EditorControl editor;
        private Timer timer;
        private bool timerDraw;
        private Bitmap timerBitmap;
        private Graphics bmpGraphics;
        private int caretX;
        private int caretY;
        private int bitmapW;
        private int bitmapH;
        
        public CaretRenderer(EditorControl editor)
        {
            this.editor = editor;
            this.timer = new Timer();
            timer.Interval = INTERVAL;
            timer.Tick += Tick;
            timer.Start();
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            Reset();
        }

        internal void Reset()
        {
            if (bmpGraphics != null)
            {
                bmpGraphics.Dispose();
                bmpGraphics = null;
            }

            if (timerBitmap != null)
            {
                timerBitmap.Dispose();
                timerBitmap = null;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if ((!editor.Focused && timerDraw)
                || editor.Buffer == null
                || OverlapRectangle(editor.CallTips.TipRectangle)
                || editor.ReadOnly)
                return;

            using (var g = editor.CreateGraphics())
            {
                g.TranslateTransform(editor.Scroll.ScrollPosition.X, editor.Scroll.ScrollPosition.Y);

                if (timerDraw && timerBitmap != null)
                    DrawSurface(g, caretX, caretY);
                else
                    DrawCaret(g, caretX, caretY, true);
            }

            timerDraw = !timerDraw;
        }

        private bool OverlapRectangle(Rectangle rect)
        {
            var cx = caretX;
            var cy = caretY;
            return !rect.IsEmpty
                && cx + editor.Info.CharWidth >= rect.X && cx <= rect.X + rect.Width
                && cy >= rect.Y && cy <= rect.Y + rect.Height;
        }

        public void Suspend()
        {
            if (timer.Enabled)
                timer.Enabled = false;

            timerDraw = true;
        }

        public void Resume()
        {
            if (!timer.Enabled)
                timer.Enabled = true;
        }

        public Graphics GetDrawingSurface()
        {
            var w = editor.Info.CharWidth;
            var h = editor.Info.LineHeight;

            if ((w != bitmapW || h != bitmapH) && timerBitmap != null)
                Reset();

            if (timerBitmap == null)
            {
                bitmapW = w;
                bitmapH = h;
                timerBitmap = new Bitmap(w, h);
                bmpGraphics = Graphics.FromImage(timerBitmap);
            }

            return bmpGraphics;
        }

        private void DrawSurface(Graphics g, int x, int y)
        {
            if (x >= editor.Info.TextLeft - editor.Scroll.ScrollPosition.X
                && x < editor.Info.TextRight - editor.Scroll.ScrollPosition.X - editor.Info.CharWidth
                && y + editor.Info.LineHeight <= editor.Info.TextBottom - editor.Scroll.ScrollPosition.Y)
            {
                g.DrawImage(timerBitmap, caretX, caretY);
            }
        }

        public void DrawCaret(Graphics g, int x, int y, bool main)
        {
            var w = GetCaretWidth(g);
            var h = GetCaretHeight(g);

            if (!main && !BlockCaret)
                w /= 2;

            if (!main && BlockCaret)
                h /= 2;

            if (x >= editor.Info.TextLeft - editor.Scroll.ScrollPosition.X
                && x < editor.Info.TextRight - editor.Scroll.ScrollPosition.X
                && y + editor.Info.LineHeight <= editor.Info.TextBottom - editor.Scroll.ScrollPosition.Y)
            {
                if (main)
                {
                    caretX = x;
                    caretY = y;
                }

                var cs = editor.Theme.GetStyle(StandardStyle.Caret);
                g.FillRectangle(cs.ForeColor.Brush(), x, 
                    BlockCaret ? y + editor.Info.CharHeight : y, w, h);
            }
        }

        private int GetCaretWidth(Graphics g)
        {
            return BlockCaret ?
                editor.Info.CharWidth : Dpi.GetWidth(ThinCaret ? 1 : 2);
        }

        private int GetCaretHeight(Graphics g)
        {
            return BlockCaret ? Dpi.GetHeight(ThinCaret ? 1 : 2) : editor.Info.LineHeight;
        }

        public int BlinkInterval => timer.Interval;

        public bool ThinCaret { get; set; }

        private bool _blockCaret;
        public bool BlockCaret
        {
            get { return _blockCaret; }
            set
            {
                if (value != _blockCaret)
                {
                    if (timerBitmap != null)
                    {
                        bmpGraphics.Dispose();
                        bmpGraphics = null;
                        timerBitmap.Dispose();
                        timerBitmap = null;
                    }

                    _blockCaret = value;
                }
            }
        }
    }
}
