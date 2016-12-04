using CodeBox.Drawing;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBox.Test
{
    public class SplitContainerEx : SplitContainer
    {
        private bool dragging;

        public SplitContainerEx()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            SplitterWidth = Dpi.GetHeight(1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!IsSplitterFixed)
            {
                dragging = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Orientation.Equals(Orientation.Vertical))
                    {
                        if (e.X > 0 && e.X < Width)
                            SplitterDistance = e.X;
                    }
                    else if (e.Y > 0 && e.Y < Height)
                        SplitterDistance = e.Y;
                }
                else
                    dragging = false;
            }

            base.OnMouseMove(e);
        }
    }
}
