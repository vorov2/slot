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
            margin.SizeChanged += MarginSizeChanged;
            margins.Add(margin);
            editor.Redraw();
        }

        private void MarginSizeChanged(object sender, EventArgs e)
        {
            _totalWidth = null;
        }

        public void Insert(int index, Margin margin)
        {
            margin.SizeChanged += MarginSizeChanged;
            margins.Insert(index, margin);
            editor.Redraw();
        }

        public void Remove(Margin margin)
        {
            margin.SizeChanged -= MarginSizeChanged;
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

        private int? _totalWidth;
        public int TotalWidth
        {
            get
            {
                if (_totalWidth == null || (_totalWidth == 0 && margins.Count > 0))
                {
                    var w = 0;

                    foreach (var m in margins)
                        w += m.CalculateSize();

                    _totalWidth = w;
                }

                return _totalWidth.Value;
            }
        }
    }
}
