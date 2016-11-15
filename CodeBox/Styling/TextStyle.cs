using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public class TextStyle : Style
    {
        public TextStyle()
        {
            
        }

        internal override Style Combine(Style other)
        {
            var hidden = other.Clone();
            hidden.ForeColor = ForeColor.IsEmpty ? other.ForeColor : ForeColor;
            hidden.BackColor = BackColor.IsEmpty ? other.BackColor : BackColor;
            hidden.FontStyle = FontStyle == FontStyle.Regular ? other.FontStyle : FontStyle;
            //hidden.ForeColor = other.ForeColor.IsEmpty ? ForeColor : other.ForeColor;
            //hidden.BackColor = other.BackColor.IsEmpty ? BackColor : other.BackColor;
            //hidden.FontStyle = other.FontStyle == FontStyle.Regular ? FontStyle : other.FontStyle;
            return hidden;
        }
    }
}
