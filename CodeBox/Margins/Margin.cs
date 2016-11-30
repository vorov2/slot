using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;

namespace CodeBox.Margins
{
    public abstract class Margin
    {
        protected Margin(Editor editor)
        {
            Editor = editor;
        }

        public virtual void Reset()
        {

        }

        public virtual MarginEffects MouseDown(Point loc) => MarginEffects.None;

        public virtual MarginEffects MouseUp(Point loc) => MarginEffects.None;

        public virtual MarginEffects MouseMove(Point loc) => MarginEffects.None;

        public bool Draw(Graphics g, Rectangle bounds)
        {
            Bounds = bounds;
            return OnDraw(g, bounds);
        }

        protected abstract bool OnDraw(Graphics g, Rectangle bounds);

        public abstract int CalculateSize();

        protected Editor Editor { get; }

        internal Rectangle Bounds { get; private set;  }

        public event EventHandler SizeChanged;
        protected virtual void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
    }
}
