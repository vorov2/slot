using CodeBox.Drawing;
using CodeBox.Affinity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Indentation;

namespace CodeBox
{
    public sealed class EditorSettings : IDocumentAffinity
    {
        private const string SEPS = "`~!@#$%^&*()-=+[{]}\\|;:'\",.<>/?";
        private const string BRACKETS = "()[]{}";
        private const int INDENTSIZE = 4;
        private readonly Editor editor;

        public EditorSettings(Editor editor)
        {
            this.editor = editor;

            //Defaults
            NonWordSymbols = SEPS;
            BracketSymbols = BRACKETS;
            //WordWrap = true;
            WordWrapColumn = 80;
            //UseTabs = true;
            MatchBrackets = true;
            ShowLineLength = true;
            CurrentLineIndicator = true;
            CurrentLineIndicatorColor = ColorTranslator.FromHtml("#262626");//161616
            IndentSize = INDENTSIZE;
            LinePadding = .1;
            ShowWhitespace = true;
            Font = new Font("Consolas", 11f);
            CaretColor = Color.White;
            ScrollThumbColor = ColorTranslator.FromHtml("#505050");
            ScrollActiveThumbColor = Color.White;
            LongLineIndicators = new List<int>();
            LongLineIndicators.AddRange(new int[] { 25, 80, 100 });
        }

        #region IDocumentAffinity
        public string NonWordSymbols { get; set; }

        public string BracketSymbols { get; set; }

        public NumberLiteral NumberLiteral { get; set; }

        string IDocumentAffinity.CommentMask { get; }

        public string IndentComponentKey { get; set; }
        #endregion

        public bool MatchBrackets
        {
            get { return editor.MatchBrakets.Enabled; }
            set
            {
                if (value != editor.MatchBrakets.Enabled)
                {
                    editor.MatchBrakets.Enabled = value;

                    if (editor.Buffer != null)
                        editor.Styles.Restyle();
                }
            }
        }

        public bool WordWrap { get; set; }

        public int WordWrapColumn { get; set; }

        public Eol Eol { get; set; }

        public bool UseTabs { get; set; }

        public int IndentSize { get; set; }

        public double LinePadding { get; set; }

        public bool ShowEol { get; set; }

        public bool ShowWhitespace { get; set; }

        public bool ShowLineLength { get; set; }

        public bool CurrentLineIndicator { get; set; }

        public Color CurrentLineIndicatorColor { get; set; }

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

                    using (var g = editor.CreateGraphics())
                    {
                        var size1 = g.MeasureString("<F>", value);
                        var size2 = g.MeasureString("<>", value);
                        editor.Info.SmallCharWidth = (int)(size1.Width - size2.Width);
                        editor.Info.SmallCharHeight = (int)value.GetHeight(g);
                    }
                }
            }
        }

        public Color CaretColor { get; set; }

        public Color ScrollThumbColor { get; set; }

        public Color ScrollActiveThumbColor { get; set; }

        public List<int> LongLineIndicators { get; }

        #region Styles
        public Color FoldingBackColor { get; set; }

        public Color FoldingForeColor { get; set; }

        public Color FoldingActiveForeColor { get; set; }
        
        public Color PopupBackColor { get; set; }

        public Color PopupBorderColor { get; set; }

        public Color PopupForeColor { get; set; }

        public Color PopupHoverColor { get; set; }

        public Color PopupSelectedColor { get; set; }

        public Color LineNumbersBackColor { get; set; }

        public Color LineNumbersForeColor { get; set; }

        public Color LineNumbersCurrentForeColor { get; set; }

        public Color LineNumbersCurrentBackColor { get; set; }
        #endregion
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
