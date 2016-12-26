using System;

namespace Slot.Editor
{
    public sealed class EditorInfo
    {
        private readonly EditorControl editor;

        internal EditorInfo(EditorControl editor)
        {
            this.editor = editor;
        }

        public int TextLeft => editor.LeftMargins.TotalWidth;

        public int TextTop => editor.TopMargins.TotalWidth;

        public int TextRight => editor.ClientSize.Width - editor.RightMargins.TotalWidth;

        public int TextBottom => editor.ClientSize.Height - editor.BottomMargins.TotalWidth;
        
        public int TextHeight => TextBottom - TextTop;

        public int TextWidth => TextRight - TextLeft;

        public int TextIntegralHeight => (TextHeight / LineHeight) * LineHeight - editor.Scroll.ScrollPosition.Y;

        public int StripesPerScreen => TextHeight / LineHeight;

        public int CharWidth => editor.EditorSettings.CharWidth;

        public int CharHeight => editor.EditorSettings.CharHeight;

        public int SmallCharWidth => editor.EditorSettings.SmallCharWidth;

        public int SmallCharHeight => editor.EditorSettings.SmallCharHeight;

        public int LineHeight =>
            CharHeight + (int)Math.Round(CharHeight * editor.EditorSettings.LinePadding);
    }
}
