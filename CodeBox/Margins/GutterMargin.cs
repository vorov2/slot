using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Commands;

namespace CodeBox.Margins
{
    public class GutterMargin : Margin
    {
        public GutterMargin(Editor editor) : base(editor)
        {

        }

        public override MarginEffects MouseDown(Point loc)
        {
            var sel = Editor.Buffer.Selections.Main;
            var lineIndex = Editor.Locations.FindLineByLocation(loc.Y);
            Editor.Commands.Run<SelectLineCommand>(new CommandArgument(new Pos(lineIndex, 0)));
            return MarginEffects.Redraw;
        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(Editor.Styles.Default.BackBrush, bounds);
            return true;
        }

        public override int CalculateSize() => Editor.Info.CharWidth;
    }
}
