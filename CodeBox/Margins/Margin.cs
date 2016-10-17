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

        public virtual void Click(int x, int y, int lineIndex, EditorContext context)
        {
            
        }

        public abstract bool Draw(Graphics g, Rectangle bounds, EditorContext context);

        public abstract int Width { get; }
    }
}
