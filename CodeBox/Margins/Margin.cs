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
        protected Margin()
        {

        }

        public virtual MarginEffects MouseDown(Point loc, EditorContext ctx)
        {
            return MarginEffects.None;
        }

        public virtual MarginEffects MouseUp(Point loc, EditorContext ctx)
        {
            return MarginEffects.None;
        }

        public virtual MarginEffects MouseMove(Point loc, EditorContext ctx)
        {
            return MarginEffects.None;
        }

        public abstract bool Draw(Graphics g, Rectangle bounds, EditorContext ctx);

        public abstract int CalculateSize(EditorContext ctx);
    }
}
