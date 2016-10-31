using CodeBox.Folding;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
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

        public event EventHandler<FoldingNeededEventArgs> FoldingNeeded;
        private void OnFoldingNeeded(Range range)
        {
            FoldingNeeded?.Invoke(this, new FoldingNeededEventArgs(range));
        }
    }
}
