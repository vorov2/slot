using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox
{
    internal sealed class EditorCaret : IDisposable
    {
        private const int INTERVAL = 500;
        
        private readonly Editor editor;
        private Timer timer;
        private bool timerDraw;
        private Bitmap timerBitmap;
        private Graphics bmpGraphics;
        private int caretX;
        private int caretY;
        
        public EditorCaret(Editor editor)
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
            if (!editor.Focused)
                return;

            using (var g = editor.CreateGraphics())
            {
                g.TranslateTransform(editor.scrollX, editor.scrollY);

                if (timerDraw && timerBitmap != null)
                    g.DrawImage(timerBitmap, caretX, caretY);
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

        public void Resume()
        {
            timer.Enabled = true;
        }

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

        public void DrawCaret(Graphics g, int x, int y, bool main)
        {
            var w = GetCaretWidth(g);
            var h = GetCaretHeight(g);

            if (!main && !BlockCaret)
                w /= 2;

            if (!main && BlockCaret)
                h /= 2;

            if (x >= editor.Info.LeftMargin - editor.scrollX
                && x < editor.Info.LeftMargin + editor.Info.ClientWidth - editor.scrollX)
            {
                if (main)
                {
                    caretX = x;
                    caretY = y;
                }

                g.FillRectangle(editor.Renderer.GetBrush(Editor.ForegroundColor), x, 
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

        public int BlinkInterval
        {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

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
