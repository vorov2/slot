using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace CodeBox.Folding
{
    public sealed class FoldingManager
    {
        private readonly Editor editor;
        private volatile bool busy;

        internal FoldingManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ExpandAll()
        {
            foreach (var ln in Lines)
                ln.Folding &= ~FoldingStates.Invisible;
        }

        public void CollapseAll()
        {
            foreach (var ln in Lines)
                if (!ln.Folding.Has(FoldingStates.Header))
                    ln.Folding |= FoldingStates.Invisible;
        }

        public bool IsCollapsedHeader(int lineIndex)
        {
            if (lineIndex + 1 >= Lines.Count)
                return false;

            var ln = Lines[lineIndex];
            return ln.Folding.Has(FoldingStates.Header) && lineIndex < Lines.Count
                && Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible);
        }

        public void ToggleExpand(int lineIndex)
        {
            var ln = Lines[lineIndex];
            var vis = lineIndex < Lines.Count
                ? Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible) : true;

            if (ln.Folding.Has(FoldingStates.Header))
            {
                var foldLevel = ln.FoldingLevel;
                var selPos = new Pos(lineIndex, ln.Length);

                for (var i = lineIndex + 1; i < Lines.Count; i++)
                {
                    var cln = Lines[i];

                    if (cln.FoldingLevel <= foldLevel && cln.FoldingLevel != 0)
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
                            var start = s.Start > s.End ? s.End : s.Start;
                            var end = s.Start > s.End ? s.Start : s.End;

                            if (i >= start.Line && i <= s.End.Line)
                                s.Clear(selPos);
                        }
                    }
                }
            }
        }

        internal void RebuildFolding(bool full = false)
        {
            if (Lines.Count == 0 || busy)
                return;

            RunRebuild(
                full ? 0 : editor.Scroll.FirstVisibleLine,
                full ? Lines.Count - 1 : editor.Scroll.LastVisibleLine);
        }

        private void RunRebuild(int fvl, int lvl)
        {
            try
            {
                busy = true;

                while (fvl > -1 && !IsFoldingHeader(fvl))
                    fvl--;

                fvl = fvl < 0 ? 0 : fvl;
                lvl = lvl < fvl ? fvl : lvl;

                var range = new Range(
                        new Pos(fvl, 0),
                        new Pos(lvl, Lines[lvl].Length - 1));

                if (FoldingNeeded != null)
                    FoldingNeeded?.Invoke(this, new FoldingNeededEventArgs(range));
                else
                {
                    var aff = editor.AffinityManager.GetRootAffinity();
                    var key = aff?.FoldingComponentKey ?? editor.Settings.FoldingComponentKey;

                    if (key != null)
                    {
                        var fp = ComponentCatalog.Instance.GetComponent<IFoldingComponent>(key);
                        fp.Fold(editor, range);
                    }
                }
            }
            finally
            {
                busy = false;
            }
        }

        public void SetFoldingLevel(int line, int level)
        {
            var ln = Lines[line];
            ln.Folding &= ~FoldingStates.Header;
            ln.FoldingLevel = (byte)(level + 1);
        }

        public void SetFoldingHeader(int line)
        {
            var ln = Lines[line];
            ln.Folding = FoldingStates.Header
                | (ln.Folding.Has(FoldingStates.Invisible) ? FoldingStates.Invisible : FoldingStates.None);
        }

        public bool IsFoldingHeader(int line) => Lines[line].Folding.Has(FoldingStates.Header);

        public event EventHandler<FoldingNeededEventArgs> FoldingNeeded;

        internal List<Line> Lines => editor.Lines;
    }
}
