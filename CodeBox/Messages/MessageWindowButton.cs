using Slot.Core.Themes;
using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Editor.Messages
{
    public sealed class MessageWindowButton : Control
    {
        private readonly Style style;

        public MessageWindowButton(Style style)
        {
            this.style = style;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
        }


    }
}
