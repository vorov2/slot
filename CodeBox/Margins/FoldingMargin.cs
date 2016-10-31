using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;

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
            {
                var ln = Editor.Lines[lineIndex];
                var vis = lineIndex < Editor.Lines.Count 
                    ? Editor.Lines[lineIndex + 1].Visible.Has(VisibleStates.Invisible) : true;

                if (ln.Visible.Has(VisibleStates.Header))
                {
                    var sw = 0;
                    var selPos = new Pos(lineIndex, ln.Length);

                    for (var i = lineIndex + 1; i < Editor.Lines.Count; i++)
                    {
                        var cln = Editor.Lines[i];

                        if (cln.Visible.Has(VisibleStates.Header))
                            sw++;
                        else if (cln.Visible.Has(VisibleStates.Footer) && sw != 0)
                            sw--;
                        else if (cln.Visible.Has(VisibleStates.Footer) && sw == 0)
                            break;

                        if (vis)
                        {
                            cln.Visible &= ~VisibleStates.Invisible;
                        }
                        else
                        {
                            cln.Visible |= VisibleStates.Invisible;

                            foreach (var s in Editor.Buffer.Selections)
                            {
                                if (s.Start.Line == i || s.End.Line == i)
                                    s.Clear(selPos);
                            }
                        }
                    }
                }
            }

            return MarginEffects.Redraw | MarginEffects.Invalidate | MarginEffects.CaptureMouse;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.Styles.FoldingMarker.BackBrush, bounds);
            var lp = (int)(Editor.Info.CharWidth * .5);
            var h = (Editor.Info.LineHeight / 2) * 2;
            var w = ((bounds.Width - lp) / 2) * 2;
            var side = w > h ? h : w;
            var x = bounds.X + lp + (w - side) / 2;

            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var ln = Editor.Lines[i];
                var y = ln.Y + Editor.Info.TextTop + Editor.Scroll.Y;
                y += (h - side) / 2;

                if (ln.Visible.Has(VisibleStates.Header) && !ln.Visible.Has(VisibleStates.Invisible))
                {
                    var arrow = default(Point[]);

                    if (!Editor.Lines[i + 1].Visible.Has(VisibleStates.Invisible))
                    {
                        arrow = new Point[]
                        {
                            new Point(x, y),
                            new Point(x + side/2, y + side/2),
                            new Point(x, y + side)
                        };
                    }
                    else
                    {
                        arrow = new Point[]
                        {
                            new Point(x, y + side/2),
                            new Point(x + side, y + side/2),
                            new Point(x + side/2, y + side)
                        };
                    }

                    g.FillPolygon(Editor.Styles.FoldingMarker.ForeBrush, arrow);
                }
            }

            return true;
        }

        public override int CalculateSize()
        {
            return (int)Math.Round(Editor.Info.CharWidth * 2.5, MidpointRounding.AwayFromZero);
        }
    }
}
