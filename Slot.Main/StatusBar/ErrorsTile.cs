using Slot.Core.Themes;
using Slot.Drawing;
using System.Drawing;
using Slot.Editor;

namespace Slot.Main.StatusBar
{
    public sealed class ErrorsTile : StatusBarTile
    {
        private readonly EditorControl editor;

        public ErrorsTile(EditorControl editor) : base(TileAlignment.Left)
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
