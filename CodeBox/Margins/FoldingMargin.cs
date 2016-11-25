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
using CodeBox.Drawing;
using CodeBox.Core;

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

            if (lineIndex > -1 && Editor.Folding.IsFoldingHeader(lineIndex))
                Editor.RunCommand((Identifier)"editor.togglefolding", lineIndex + 1);
            else
                Editor.RunCommand((Identifier)"editor.selectline", lineIndex + 1);

            return MarginEffects.Redraw | MarginEffects.Invalidate | MarginEffects.CaptureMouse;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var fs = (MarginStyle)Editor.Styles.Theme.GetStyle(StandardStyle.Folding);
            g.FillRectangle(fs.BackColor.Brush(), bounds);
            var lp = (int)(Editor.Info.CharWidth * .5);
            var h = (Editor.Info.LineHeight / 2) * 2;
            var w = ((bounds.Width - lp*2) / 2) * 2;
            var side = w > h ? h : w;
            var x = bounds.X + lp + (w - side) / 2;

            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var ln = Editor.Lines[i];
                var y = ln.Y + Editor.Info.TextTop + Editor.Scroll.ScrollPosition.Y;
                y += (h - side) / 2;

                if (ln.Folding.Has(FoldingStates.Header) && !ln.Folding.Has(FoldingStates.Invisible))
                {
                    var arrow = default(Point[]);
                    var b = fs.ForeColor.Brush();

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
                        b = fs.ActiveForeColor.Brush();
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
