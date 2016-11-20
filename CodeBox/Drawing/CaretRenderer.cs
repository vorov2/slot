using CodeBox.Styling;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBox.Drawing
{
    internal sealed class CaretRenderer : IDisposable
    {
        private const int INTERVAL = 500;
        
        private readonly Editor editor;
        private Timer timer;
        private bool timerDraw;
        private Bitmap timerBitmap;
        private Graphics bmpGraphics;
        private int caretX;
        private int caretY;
        
        public CaretRenderer(Editor editor)
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

            if (bmpGraphics != null)
            {
                bmpGraphics.Dispose();
                bmpGraphics = null;
            }
            
            if (bmpGraphics != null)
            {
                bmpGraphics.Dispose();
                bmpGraphics = null;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (!editor.Focused && timerDraw)
                return;

            var rect = editor.CallTips.TipRectangle;
            var cx = caretX + editor.Scroll.ScrollPosition.X;
            var cy = caretY + editor.Scroll.ScrollPosition.Y;

            if (cx >= rect.X && cx <= rect.X + rect.Width
                && cy >= rect.Y && cy <= rect.Y + rect.Height)
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

        public void Suspend()
        {
            timer.Enabled = false;
            timerDraw = true;
        }

        public void Resume() => timer.Enabled = true;

        public Graphics GetDrawingSurface()
        {
            var w = editor.Info.CharWidth;
            var h = editor.Info.LineHeight;
            
            if (timerBitmap == null)
            {
                timerBitmap = new Bitmap(w, h);
                bmpGraphics = Graphics.FromImage(timerBitmap);
            }

            return bmpGraphics;
        }

        private void DrawSurface(Graphics g, int x, int y)
        {
            if (x >= editor.Info.TextLeft - editor.Scroll.ScrollPosition.X
                && x < editor.Info.TextRight - editor.Scroll.ScrollPosition.X
                && y + editor.Info.LineHeight < editor.Info.TextBottom - editor.Scroll.ScrollPosition.Y)
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
                && y + editor.Info.LineHeight < editor.Info.TextBottom - editor.Scroll.ScrollPosition.Y)
            {
                if (main)
                {
                    caretX = x;
                    caretY = y;
                }

                var cs = editor.Styles.Styles.GetStyle(StandardStyle.Caret);
                g.FillRectangle(cs.ForeColor.Brush(), x, 
                    BlockCaret ? y + editor.Info.LineHeight - h : y, w, h);
            }
        }

        private int GetCaretWidth(Graphics g)
        {
            return BlockCaret ?
                editor.Info.CharWidth :
                (int)Math.Round(g.DpiX / 96f) * 2;
        }

        private int GetCaretHeight(Graphics g)
        {
            return BlockCaret ?
                (int)Math.Round(g.DpiY / 96f) * 2 :
                editor.Info.LineHeight;
        }

        public int BlinkInterval => timer.Interval;

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
