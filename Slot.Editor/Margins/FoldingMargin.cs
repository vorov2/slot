using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Editor.Styling;
using Slot.Editor.Folding;
using Slot.Editor.Commands;
using Slot.Drawing;
using Slot.Core;
using Slot.Core.Themes;

namespace Slot.Editor.Margins
{
    public class FoldingMargin : Margin
    {
        public FoldingMargin(EditorControl editor) : base(editor)
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
            var fs = Editor.Theme.GetStyle(StandardStyle.Folding);
            var afs = Editor.Theme.GetStyle(StandardStyle.ActiveFolding);
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

                if (ln.Folding.Has(FoldingStates.Header) && Editor.Folding.IsLineVisible(i))
                {
                    var arrow = default(Point[]);
                    var b = fs.ForeColor.Brush();

                    if (Editor.Lines.Count > i + 1 && Editor.Folding.IsLineVisible(i + 1))
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
                        b = afs.ForeColor.Brush();
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
