using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Folding;
using CodeBox.Commands;

namespace CodeBox.Margins
{
    public class FoldingMargin : Margin
    {
        public FoldingMargin(Editor editor) : base(editor)
        {

        }

        public override MarginEffects MouseDown(Point loc)
        {
            var lineIndex = Editor.Locations.FindLineByLocation(loc.Y);

            if (lineIndex > -1)
                Editor.Commands.Run(new ToggleFoldingCommand(new Pos(lineIndex, 0)));

            return MarginEffects.Redraw | MarginEffects.Invalidate | MarginEffects.CaptureMouse;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.CachedBrush.Create(Editor.Settings.FoldingBackColor), bounds);
            var lp = (int)(Editor.Info.CharWidth * .5);
            var h = (Editor.Info.LineHeight / 2) * 2;
            var w = ((bounds.Width - lp*2) / 2) * 2;
            var side = w > h ? h : w;
            var x = bounds.X + lp + (w - side) / 2;

            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var ln = Editor.Lines[i];
                var y = ln.Y + Editor.Info.TextTop + Editor.Scroll.Y;
                y += (h - side) / 2;

                if (ln.Folding.Has(FoldingStates.Header) && !ln.Folding.Has(FoldingStates.Invisible))
                {
                    var arrow = default(Point[]);
                    var b = Editor.CachedBrush.Create(Editor.Settings.FoldingForeColor);

                    if (Editor.Lines.Count > i + 1 && !Editor.Lines[i + 1].Folding.Has(FoldingStates.Invisible))
                    {
                        arrow = new Point[]
                        {
                            new Point(x, y + side/2),
                            new Point(x + side, y + side/2),
                            new Point(x + side/2, y + side)
                        };
                    }
                    else
                    {
                        b = Editor.CachedBrush.Create(Editor.Settings.FoldingActiveForeColor);
                        arrow = new Point[]
                        {
                            new Point(x, y),
                            new Point(x + side/2, y + side/2),
                            new Point(x, y + side)
                        }; 
                    }

                    g.FillPolygon(b, arrow);
                }
            }

            return true;
        }

        public override int CalculateSize() =>
            (int)Math.Round(Editor.Info.CharWidth * 2.5, MidpointRounding.AwayFromZero);
    }
}
