using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public sealed class TextStyle : Style
    {
        private static readonly TextStyle hidden = new TextStyle();

        public TextStyle()
        {
            
        }

        internal override Style Combine(Style other)
        {
            var hidden = other.Clone();
            hidden.ForeColor = ForeColor.IsEmpty ? other.ForeColor : ForeColor;
            hidden.BackColor = BackColor.IsEmpty ? other.BackColor : BackColor;
            hidden.FontStyle = FontStyle == FontStyle.Regular ? other.FontStyle : FontStyle;
            return hidden;
        }
    }
}
