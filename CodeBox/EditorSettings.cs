using CodeBox.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class EditorSettings
    {
        private const string SEPS = "`~!@#$%^&*()-=+[{]}\\|;:'\",.<>/?";
        private const int TABSIZE = 2;
        private readonly Editor editor;

        public EditorSettings(Editor editor)
        {
            this.editor = editor;

            //Defaults
            WordSeparators = SEPS;
            //WordWrap = true;
            //UseTabs = true;
            TabSize = TABSIZE;
            LinePadding = .1;
            ShowWhitespace = true;
            Font = new Font("Consolas", 11f);
            CaretColor = Color.White;
            ScrollThumbColor = ColorTranslator.FromHtml("#505050");
            ScrollActiveThumbColor = Color.White;
        }

        public string WordSeparators { get; set; }

        public bool WordWrap { get; set; }

        public Eol Eol { get; set; }

        public bool UseTabs { get; set; }

        public int TabSize { get; set; }

        public double LinePadding { get; set; }

        public bool ShowEol { get; set; }

        public bool ShowWhitespace { get; set; }

        public Font Font
        {
            get { return editor.Font; }
            set
            {
                if (value != editor.Font)
                {
                    editor.Font = value;

                    if (editor.CachedFont != null)
                        editor.CachedFont.Dispose();

                    editor.CachedFont = new CachedFont(value);
                    SmallFont = new Font(value.Name, value.Size - 1, value.Style);

                    using (var g = editor.CreateGraphics())
                    {
                        var size1 = g.MeasureString("<F>", value);
                        var size2 = g.MeasureString("<>", value);
                        editor.Info.CharWidth = (int)(size1.Width - size2.Width);
                        editor.Info.CharHeight = (int)value.GetHeight(g);
                    }
                }
            }
        }

        private Font _smallFont;
        internal Font SmallFont
        {
            get { return _smallFont; }
            private set
            {
                if (value != _smallFont)
                {
                    _smallFont = value;

                    if (editor.CachedSmallFont != null)
                        editor.CachedSmallFont.Dispose();

                    editor.CachedSmallFont = new CachedFont(value);
                }
            }
        }

        public Color CaretColor { get; set; }

        public Color ScrollThumbColor { get; set; }

        public Color ScrollActiveThumbColor { get; set; }
    }

    public enum Eol
    {
        Auto = 0,

        Cr,

        Lf,

        CrLf
    }

    internal static class EolExtensions
    {
        public static string AsString(this Eol eol)
        {
            switch (eol)
            {
                case Eol.Cr: return "\r";
                case Eol.Lf: return "\n";
                case Eol.CrLf: return "\r\n";
                default: return Environment.NewLine;
            }
        }
    }
}
