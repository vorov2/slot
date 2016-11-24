using System;

namespace CodeBox
{
    public sealed class TextEventArgs : EventArgs
    {
        public TextEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
