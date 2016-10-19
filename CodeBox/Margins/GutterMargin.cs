using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public class GutterMargin : Margin
    {
        public GutterMargin(Editor editor) : base(editor)
        {

        }

        public override MarginEffects MouseDown(Point loc)
        {
            Editor.Document.Selections.Clear();
            var sel = Editor.Document.Selections.Main;
            var lineIndex = Editor.FindLineByLocation(loc.Y);

            if (lineIndex >= 0)
            {
                sel.Start = new Pos(lineIndex, 0);
                sel.End = new Pos(lineIndex, Editor.Document.Lines[lineIndex].Length);
            }

            return MarginEffects.Redraw;
        }

        public override bool Draw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.CachedBrush.Create(Editor.BackgroundColor), bounds);
            return true;
        }

        public override int CalculateSize()
        {
            return Editor.Info.CharWidth;
        }
    }
}
