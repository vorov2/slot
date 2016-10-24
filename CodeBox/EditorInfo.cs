using System;

namespace CodeBox
{
    public sealed class EditorInfo
    {
        private readonly Editor editor;

        internal EditorInfo(Editor editor)
        {
            this.editor = editor;
        }

        public int TextLeft
        {
            get { return editor.LeftMargins.TotalWidth; }
        }

        public int TextTop
        {
            get { return editor.TopMargins.TotalWidth; }
        }

        public int TextRight
        {
            get { return editor.ClientSize.Width - editor.RightMargins.TotalWidth; }
        }

        public int TextBotom
        {
            get { return editor.ClientSize.Height - editor.BottomMargins.TotalWidth; }
        }
        
        public int TextHeight
        {
            get { return TextBotom - TextTop; }
        }

        public int TextWidth
        {
            get { return TextRight - TextLeft; }
        }

        public int TextIntegralHeight
        {
            get { return (TextHeight / LineHeight) * LineHeight - editor.Scroll.Y; }
        }

        public int StripesPerScreen
        {
            get { return TextHeight / LineHeight; }
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
