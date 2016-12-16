using System;
using System.Drawing;

namespace Slot.Editor.Margins
{
    public abstract class Margin
    {
        protected Margin(EditorControl editor)
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

        protected EditorControl Editor { get; }

        internal Rectangle Bounds { get; private set;  }

        public event EventHandler SizeChanged;
        protected virtual void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
    }
}
