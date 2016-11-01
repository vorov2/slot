using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Styling
{
    public sealed class PopupStyle : TextStyle
    {
        public PopupStyle()
        {

        }

        internal override Font Font
        {
            get { return Editor.CachedSmallFont.Create(FontStyle); }
        }

        internal Color BorderColor
        {
            get { return ControlPaint.Light(BackColor); }
        }
    }
}
