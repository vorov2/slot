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
        private const int TABSIZE = 4;
        private readonly Editor editor;

        public EditorSettings(Editor editor)
        {
            this.editor = editor;

            //Defaults
            WordSeparators = SEPS;
            //WordWrap = true;
            UseTabs = false;
            TabSize = 4;
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
}
