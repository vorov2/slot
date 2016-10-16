using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class EditorInfo
    {
        private readonly Editor editor;

        internal EditorInfo(Editor editor)
        {
            this.editor = editor;
        }

        public int LeftMargin
        {
            get { return editor.LeftMargins.TotalWidth; }
        }

        public int TopMargin
        {
            get { return 0; } //Temp
        }

        public int ClientWidth
        {
            get { return editor.ClientSize.Width - LeftMargin; }
        }

        public int ClientHeight
        {
            get { return editor.ClientSize.Height - TopMargin; } //Temp
        }

        public int IntegralHeight
        {
            get { return (ClientHeight / LineHeight) * LineHeight - editor.AutoScrollPosition.Y; }
        }
        
        public int CharWidth
        {
            get { return editor.FontSize.Width; }
        }

        public int LineHeight
        {
            get
            {
                return editor.FontSize.Height + 
                    (int)Math.Round(editor.FontSize.Height * editor.Settings.LinePadding);
            }
        }

        public Font Font
        {
            get { return editor.font; }
        }
    }
}
