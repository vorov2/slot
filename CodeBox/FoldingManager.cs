using CodeBox.Folding;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class FoldingManager
    {
        private readonly Editor editor;

        internal FoldingManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ExpandAll()
        {
            foreach (var ln in editor.Lines)
                ln.Visible &= ~FoldingStates.Invisible;
        }

        public void CollapseAll()
        {
            foreach (var ln in editor.Lines)
                if (!ln.Visible.Has(FoldingStates.Header))
                    ln.Visible |= FoldingStates.Invisible;
        }

        internal void RebuildFolding()
        {
            if (editor.Lines.Count == 0)
                return;

            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;
            OnFoldingNeeded(
                new Range(
                    new Pos(fvl, 0),
                    new Pos(lvl, editor.Lines[lvl].Length - 1)));
        }

        internal void DrawFoldingIndicator(Graphics g, int x, int y)
        {
            g.FillRectangle(editor.Styles.ActiveFoldingMarker.ForeBrush,
                new Rectangle(x, y + editor.Info.LineHeight / 4, editor.Info.CharWidth * 3, editor.Info.LineHeight / 2));
            g.DrawString("···", editor.Styles.Default.Font, editor.Styles.ActiveFoldingMarker.BackBrush,
                new Point(x, y), TextStyle.Format);
        }

        public event EventHandler<FoldingNeededEventArgs> FoldingNeeded;
        private void OnFoldingNeeded(Range range)
        {
            Task.Run(() =>
                FoldingNeeded?.Invoke(this, new FoldingNeededEventArgs(range)));
        }
    }
}
