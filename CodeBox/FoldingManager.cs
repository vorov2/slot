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
                ln.Folding &= ~FoldingStates.Invisible;
        }

        public void CollapseAll()
        {
            foreach (var ln in editor.Lines)
                if (!ln.Folding.Has(FoldingStates.Header))
                    ln.Folding |= FoldingStates.Invisible;
        }

        public bool IsCollapsedHeader(int lineIndex)
        {
            var ln = editor.Lines[lineIndex];
            return ln.Folding.Has(FoldingStates.Header) && lineIndex < editor.Lines.Count
                && editor.Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible);
        }

        public void ToggleExpand(int lineIndex)
        {
            var ln = editor.Lines[lineIndex];
            var vis = lineIndex < editor.Lines.Count
                ? editor.Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible) : true;

            if (ln.Folding.Has(FoldingStates.Header))
            {
                var sw = 0;
                var selPos = new Pos(lineIndex, ln.Length);

                for (var i = lineIndex + 1; i < editor.Lines.Count; i++)
                {
                    var cln = editor.Lines[i];

                    if (cln.Folding.Has(FoldingStates.Header))
                        sw++;
                    else if (cln.Folding.Has(FoldingStates.Footer) && sw != 0)
                        sw--;
                    else if (cln.Folding.Has(FoldingStates.Footer) && sw == 0)
                        break;

                    if (vis)
                    {
                        cln.Folding &= ~FoldingStates.Invisible;
                    }
                    else
                    {
                        cln.Folding |= FoldingStates.Invisible;

                        foreach (var s in editor.Buffer.Selections)
                        {
                            if (s.Start.Line == i || s.End.Line == i)
                                s.Clear(selPos);
                        }
                    }
                }
            }
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
