using Slot.Drawing;
using System;
using System.Drawing;

namespace Slot.Main.StatusBar
{
    public class StatusBarTile
    {
        public StatusBarTile(TileAlignment alignment)
        {
            Alignment = alignment;
        }

        internal static readonly StringFormat measureFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        internal static readonly StringFormat drawFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        public virtual void Draw(Graphics g, Color color, Rectangle rect)
        {
            g.DrawString(Text, Font, color.Brush(), rect, drawFormat);
        }

        public virtual int MeasureWidth(Graphics g)
        {
            var size = g.MeasureString(Text, Font, int.MaxValue, measureFormat);
            return (int)Math.Round(size.Width, MidpointRounding.AwayFromZero) + Dpi.GetWidth(8);
        }

        internal protected Font Font { get; internal set; }

        internal protected virtual void PerformClick() => OnClick();

        internal int Left { get; set; }

        internal int Right { get; set; }

        internal bool Hover { get; set; }

        public virtual string Text { get; set; }

        public TileAlignment Alignment { get; }

        public virtual bool Visible { get; set; }

        public event EventHandler Click;
        protected virtual void OnClick() => Click?.Invoke(this, EventArgs.Empty);
    }
}
