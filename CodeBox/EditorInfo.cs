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

        public int EditorLeft
        {
            get { return editor.LeftMargins.TotalWidth; }
        }

        public int EditorTop
        {
            get { return editor.TopMargins.TotalWidth; }
        }

        public int EditorRight
        {
            get { return editor.ClientSize.Width - RightMargin; }
        }

        public int EditorBottom
        {
            get { return editor.ClientSize.Height - BottomMargin; }
        }

        public int RightMargin
        {
            get { return editor.RightMargins.TotalWidth; }
        }

        public int BottomMargin
        {
            get { return editor.BottomMargins.TotalWidth; }
        }

        public int EditorHeight
        {
            get { return EditorBottom - EditorTop; }
        }

        public int EditorWidth
        {
            get { return EditorRight - EditorLeft; }
        }

        public int EditorIntegralHeight
        {
            get { return (EditorHeight / LineHeight) * LineHeight - editor.Scroll.Y; }
        }

        public int StripesPerScreen
        {
            get { return EditorHeight / LineHeight; }
        }

        public int CharWidth { get; internal set; }

        public int CharHeight { get; internal set; }

        public int LineHeight
        {
            get
            {
                return CharHeight + 
                    (int)Math.Round(CharHeight * editor.Settings.LinePadding);
            }
        }
    }
}
