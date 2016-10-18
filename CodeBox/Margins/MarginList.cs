using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public sealed class MarginList : IEnumerable<Margin>
    {
        private readonly Editor editor;
        private readonly List<Margin> margins = new List<Margin>();

        internal MarginList(Editor editor)
        {
            this.editor = editor;
        }

        public void Add(Margin margin)
        {
            margins.Add(margin);
            editor.Redraw();
        }

        public void Insert(int index, Margin margin)
        {
            margins.Insert(index, margin);
            editor.Redraw();
        }

        public void Remove(Margin margin)
        {
            margins.Remove(margin);
            editor.Redraw();
        }

        public IEnumerator<Margin> GetEnumerator()
        {
            return margins.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return margins.GetEnumerator();
        }

        public int Count
        {
            get { return margins.Count; }
        }

        public int TotalWidth
        {
            get
            {
                var w = 0;

                foreach (var m in margins)
                    w += m.CalculateSize(editor.GetEditorContext());

                return w;
            }
        }
    }
}
