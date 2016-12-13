using CodeBox.Core.Themes;
using CodeBox.Drawing;
using System.Drawing;

namespace CodeBox.Main.StatusBar
{
    public sealed class ErrorsTile : StatusBarTile
    {
        private readonly Editor editor;

        public ErrorsTile(Editor editor) : base(TileAlignment.Left)
        {
            this.editor = editor;
        }

        public override void Draw(Graphics g, Color color, Rectangle rect)
        {
            var style = editor.Theme.GetStyle(StandardStyle.Error);
            g.DrawString("Error(s): 1", Font, style.ForeColor.Brush(), rect, drawFormat);
        }

        public override int MeasureWidth(Graphics g)
        {
            return editor.Info.CharWidth * "Errors: (1)".Length;
        }
    }
}
