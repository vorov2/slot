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

        public int TextLeft => editor.LeftMargins.TotalWidth;

        public int TextTop => editor.TopMargins.TotalWidth;

        public int TextRight => editor.ClientSize.Width - editor.RightMargins.TotalWidth;

        public int TextBottom => editor.ClientSize.Height - editor.BottomMargins.TotalWidth;
        
        public int TextHeight => TextBottom - TextTop;

        public int TextWidth => TextRight - TextLeft;

        public int TextIntegralHeight => (TextHeight / LineHeight) * LineHeight - editor.Scroll.Y;

        public int StripesPerScreen => TextHeight / LineHeight;

        public int CharWidth { get; internal set; }

        public int CharHeight { get; internal set; }

        public int LineHeight =>
            CharHeight + (int)Math.Round(CharHeight * editor.Settings.LinePadding);
    }
}
