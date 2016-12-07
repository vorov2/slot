using CodeBox.Drawing;
using CodeBox.StatusBar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Test
{
    public sealed class OutputToggleTile : StatusBarTile
    {
        private readonly MainForm form;

        public OutputToggleTile(MainForm form) : base(TileAlignment.Left)
        {
            this.form = form;
        }

        public override void Draw(Graphics g, Color color, Rectangle rect)
        {
            var h = (int)Math.Round(rect.Height * .6, MidpointRounding.AwayFromZero);
            g.DrawRectangle(color.Pen(),
                new Rectangle(rect.X, rect.Y + ((rect.Height - h) / 2), rect.Width, h));

            if (form.SplitContainer.Panel2Collapsed)
            {
                var ly = rect.Y + ((rect.Height - h) / 2)
                    + (int)Math.Round(h / 2f, MidpointRounding.AwayFromZero);
                g.DrawLine(color.Pen(), rect.X, ly, rect.X + rect.Width, ly);
            }
        }

        protected override void PerformClick()
        {
            form.SplitContainer.BackColor = form.SplitContainer.Panel1.BackColor
                = form.SplitContainer.Panel2.BackColor = form.Editor.BackColor;
            form.SplitContainer.Panel2Collapsed = !form.SplitContainer.Panel2Collapsed;
            form.SplitContainer.SplitterDistance = (int)(form.ClientSize.Height * 0.5);
        }

        public override int MeasureWidth(Graphics g) => form.Editor.Info.CharWidth * 2;
    }
}
