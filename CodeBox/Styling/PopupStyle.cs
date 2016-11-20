using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public sealed class PopupStyle : Style
    {
        public Color HoverColor { get; set; }

        public Color SelectedColor { get; set; }

        public Color BorderColor { get; set; }
    }
}
