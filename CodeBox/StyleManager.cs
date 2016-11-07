using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Styling;
using System.Drawing;
using CodeBox.ObjectModel;
using CodeBox.Lexing;

namespace CodeBox
{
    public sealed class StyleManager
    {
        private readonly Dictionary<int, Style> styles = new Dictionary<int, Style>();
        private readonly Editor editor;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };
        
        public StyleManager(Editor editor)
        {
            this.editor = editor;
            Register(StandardStyle.Default, new TextStyle { Default = true });
            Register(StandardStyle.Selection, new SelectionStyle());
            Register(StandardStyle.SpecialSymbol, new TextStyle());
            Register(StandardStyle.Hyperlink, new TextStyle { FontStyle = FontStyle.Underline });
            Register(StandardStyle.MatchedBracket, new TextStyle());
            Register(StandardStyle.Number, new TextStyle());
            Register(StandardStyle.Bracket, new TextStyle());
            Register(StandardStyle.Keyword, new TextStyle());
            Register(StandardStyle.KeywordSpecial, new TextStyle());
            Register(StandardStyle.KeywordType, new TextStyle());
            Register(StandardStyle.KeywordModifier, new TextStyle());
            Register(StandardStyle.Comment, new TextStyle());
            Register(StandardStyle.CommentMultiline, new TextStyle());
            Register(StandardStyle.CommentDocument, new TextStyle());
            Register(StandardStyle.Char, new TextStyle());
            Register(StandardStyle.String, new TextStyle());
            Register(StandardStyle.StringMultiline, new TextStyle());
            Register(StandardStyle.StringSplice, new TextStyle());
            Register(StandardStyle.Preprocessor, new TextStyle());
            Register(StandardStyle.Literal, new TextStyle());
            Register(StandardStyle.Regex, new TextStyle());
        }

        public Style GetStyle(int styleId) => styles[styleId];

        private void Register(StandardStyle styleId, Style style)
        {
            style.Editor = editor;
            style.Cloned = style.FullClone();
            style.Cloned.Editor = editor;
            styles.Add((int)styleId, style);

            switch (styleId)
            {
                case StandardStyle.Default:
                    Default = style;
                    break;
                case StandardStyle.SpecialSymbol:
                    SpecialSymbol = style;
                    break;
                case StandardStyle.Selection:
                    Selection = style;
                    break;
                case StandardStyle.Hyperlink:
                    Hyperlink = style;
                    break;
                case StandardStyle.MatchedBracket:
                    MatchBracket = style;
                    break;
                case StandardStyle.Number:
                    Number = style;
                    break;
                case StandardStyle.Bracket:
                    Bracket = style;
                    break;
                case StandardStyle.Keyword:
                    Keyword = style;
                    break;
                case StandardStyle.KeywordSpecial:
                    KeywordSpecial = style;
                    break;
                case StandardStyle.KeywordType:
                    KeywordType = style;
                    break;
                case StandardStyle.KeywordModifier:
                    KeywordModifier = style;
                    break;
                case StandardStyle.Comment:
                    Comment = style;
                    break;
                case StandardStyle.CommentMultiline:
                    CommentMultiline = style;
                    break;
                case StandardStyle.CommentDocument:
                    CommentDocument = style;
                    break;
                case StandardStyle.String:
                    String = style;
                    break;
                case StandardStyle.StringMultiline:
                    StringMultiline = style;
                    break;
                case StandardStyle.StringSplice:
                    StringSplice = style;
                    break;
                case StandardStyle.Char:
                    Char = style;
                    break;
                case StandardStyle.Preprocessor:
                    Preprocessor = style;
                    break;
                case StandardStyle.Literal:
                    Literal = style;
                    break;
                case StandardStyle.Regex:
                    Regex = style;
                    break;
            }
        }

        public void Register(StyleId styleId, Style style)
        {
            style.Editor = editor;
            style.Cloned = style.FullClone();
            style.Cloned.Editor = editor;
            styles.Remove(styleId);
            styles.Add(styleId, style);
        }

        public void ClearStyles(int line) => editor.Lines[line].AppliedStyles.Clear();

        public void StyleRange(int style, int line, int start, int end) =>
            editor.Lines[line].AppliedStyles.Add(new AppliedStyle(style, start, end));

        internal void Restyle()
        {
            if (editor.Lines.Count == 0)
                return;

            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;
            var state = 0;

            while (fvl > -1 && (state = editor.Lines[fvl].State) != 0)
                fvl--;

            fvl = fvl < 0 ? 0 : fvl;
            lvl = lvl < fvl ? fvl : lvl;
            var range = new Range(new Pos(fvl, 0),
                new Pos(lvl, editor.Lines[lvl].Length - 1));
            RestyleRange(range);
        }

        internal void RestyleDocument()
        {
            var range = new Range(new Pos(0, 0), new Pos(editor.Lines.Count - 1, 0));
            RestyleRange(range);
        }

        private void RestyleRange(Range range)
        {
            if (range.End.Line <= 0)
                return;

            if(Provider != null)
                Provider.Style(editor.Context, range);
            else
                OnStyleNeeded(range);
        }

        public event EventHandler<StyleNeededEventArgs> StyleNeeded;
        private void OnStyleNeeded(Range range) =>
            StyleNeeded?.Invoke(this, new StyleNeededEventArgs(range));

        public IStylingProvider Provider { get; set; }

        #region Standard styles
        public Style Default { get; private set; }

        public Style Selection { get; private set; }

        public Style SpecialSymbol { get; private set; }
        
        public Style Hyperlink { get; private set; }

        public Style MatchBracket { get; private set; }

        public Style Number { get; private set; }

        public Style Bracket { get; private set; }

        public Style Keyword { get; private set; }

        public Style KeywordSpecial { get; private set; }

        public Style KeywordType { get; private set; }

        public Style KeywordModifier { get; private set; }

        public Style Comment { get; private set; }

        public Style CommentMultiline { get; private set; }

        public Style CommentDocument { get; private set; }

        public Style String { get; private set; }

        public Style StringMultiline { get; private set; }

        public Style StringSplice { get; private set; }

        public Style Char { get; private set; }

        public Style Preprocessor { get; private set; }

        public Style Literal { get; private set; }

        public Style Regex { get; private set; }
        #endregion
    }
}
