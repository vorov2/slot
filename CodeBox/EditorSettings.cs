using CodeBox.Drawing;
using CodeBox.Affinity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeBox.Core;
using CodeBox.Core.Settings;

namespace CodeBox
{
    public sealed class EditorSettings : SettingsBag, IDocumentAffinity
    {
        private const string SEPS = "`~!@#$%^&*()-=+[{]}\\|;:'\",.<>/?";
        private const string BRACKETS = "()[]{}";

        public EditorSettings()
        {
            //Defaults
            NonWordSymbols = SEPS;
            BracketSymbols = BRACKETS;
        }

        #region IDocumentAffinity
        [Setting("editor.delimeters")]
        public string NonWordSymbols { get; set; }

        [Setting("editor.brackets")]
        public string BracketSymbols { get; set; }

        public NumberLiteral NumberLiteral { get; set; }

        string IDocumentAffinity.CommentMask { get; }

        public Identifier IndentComponentKey { get; set; }

        public Identifier FoldingComponentKey { get; set; }

        public string AutocompleteSymbols { get; set; }
        #endregion

        [Setting("editor.showLineNumbers")]
        public bool ShowLineNumbers { get; set; }

        [Setting("editor.matchWords")]
        public bool MatchWords { get; set; }

        [Setting("editor.matchBrackets")]
        public bool MatchBrackets { get; set; }

        [Setting("editor.wordWrap")]
        public bool WordWrap { get; set; }

        [Setting("editor.wordWrapColumn")]
        public int WordWrapColumn { get; set; }

        [Setting("editor.wrappingIndent")]
        public WrappingIndent WrappingIndent { get; set; }

        public Eol Eol { get; set; }

        [Setting("editor.useTabs")]
        public bool UseTabs { get; set; }

        [Setting("editor.indentSize")]
        public int IndentSize { get; set; }

        [Setting("editor.linePadding")]
        public double LinePadding { get; set; }

        [Setting("editor.showEol")]
        public bool ShowEol { get; set; }

        [Setting("editor.showWhitespace")]
        public ShowWhitespace ShowWhitespace { get; set; }

        [Setting("editor.showLineLength")]
        public bool ShowLineLength { get; set; }

        [Setting("editor.currentLineIndicator")]
        public bool CurrentLineIndicator { get; set; }

        private int? _charWidth;
        internal int CharWidth
        {
            get
            {
                if (_charWidth == null)
                    CreateFont();
                return _charWidth.Value;
            }
        }

        private int? _charHeight;
        internal int CharHeight
        {
            get
            {
                if (_charHeight == null)
                    CreateFont();
                return _charHeight.Value;
            }
        }

        private int? _smallCharWidth;
        internal int SmallCharWidth
        {
            get
            {
                if (_smallCharWidth == null)
                    CreateSmallFont();
                return _smallCharWidth.Value;
            }
        }

        private int? _smallCharHeight;
        internal int SmallCharHeight
        {
            get
            {
                if (_smallCharHeight == null)
                    CreateSmallFont();
                return _smallCharHeight.Value;
            }
        }

        private string _fontName;
        [Setting("editor.font")]
        public string FontName
        {
            get { return _fontName; }
            set
            {
                if (value != _fontName)
                {
                    ResetFontInfo();
                    _fontName = value;
                }
            }
        }

        private float _fontSize;
        [Setting("editor.fontSize")]
        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                if (value != _fontSize)
                {
                    ResetFontInfo();
                    _fontSize = value;
                }
            }
        }

        private void ResetFontInfo()
        {
            _charWidth = null;
            _charHeight = null;
            _smallCharHeight = null;
            _smallCharWidth = null;

            if (_font != null)
            {
                FontExtensions.Clean(_font);
                _font.Dispose();
                _font = null;
            }

            if (_smallFont != null)
            {
                FontExtensions.Clean(_smallFont);
                _smallFont.Dispose();
                _smallFont = null;
            }
        }

        private void CreateFont()
        {
            FontExtensions.Clean(_font);
            _font = new Font(FontName, FontSize, FontStyle.Regular);
            _charWidth = _font.Width();
            _charHeight = _font.Height();
        }

        private void CreateSmallFont()
        {
            FontExtensions.Clean(_smallFont);
            _smallFont = new Font(FontName, FontSize - 1, FontStyle.Regular);
            _smallCharWidth = _font.Width();
            _smallCharHeight = _font.Height();
        }

        private Font _font;
        public Font Font
        {
            get
            {
                if (_font == null)
                    CreateFont();

                return _font;
            }
        }

        private Font _smallFont;
        public Font SmallFont
        {
            get
            {
                if (_smallFont == null)
                    CreateSmallFont();

                return _smallFont;
            }
        }

        [Setting("editor.longLineIndicators")]
        public List<int> LongLineIndicators { get; set; }
    }

    public enum Eol
    {
        [FieldName("Auto")]
        Auto = 0,

        [FieldName("CR")]
        Cr,

        [FieldName("LF")]
        Lf,

        [FieldName("CRLF")]
        CrLf
    }

    public enum WrappingIndent
    {
        [FieldName("None")]
        None = 0,

        [FieldName("Same")]
        Same,

        [FieldName("Indent")]
        Indent
    }

    public enum ShowWhitespace
    {
        [FieldName("None")]
        None = 0,

        [FieldName("All")]
        All,

        [FieldName("Boundary")]
        Boundary
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
