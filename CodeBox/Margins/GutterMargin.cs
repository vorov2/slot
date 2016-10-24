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
    public class GutterMargin : Margin
    {
        public GutterMargin(Editor editor) : base(editor)
        {

        }

        public override MarginEffects MouseDown(Point loc)
        {
            Editor.Buffer.Selections.Truncate();
            var sel = Editor.Buffer.Selections.Main;
            var lineIndex = Editor.FindLineByLocation(loc.Y);

            if (lineIndex >= 0)
            {
                sel.Start = new Pos(lineIndex, 0);
                sel.End = new Pos(lineIndex, Editor.Document.Lines[lineIndex].Length);
            }

            return MarginEffects.Redraw;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.Styles.Default.BackBrush, bounds);
            return true;
        }

        public override int CalculateSize()
        {
            return Editor.Info.CharWidth;
        }
    }
}
