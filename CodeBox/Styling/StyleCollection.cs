using System;
using System.Collections.Generic;

namespace CodeBox.Styling
{
    public sealed class StyleCollection
    {
        private readonly Dictionary<int, Style> styles = new Dictionary<int, Style>();

        public StyleCollection()
        {
            var def = new TextStyle { Default = true };
            Register(0, def);
            DefaultStyle = def;
        }

        public Style GetStyle(int styleId) => styles[styleId];

        public Style GetStyle(StandardStyle style)
        {
            Style ret;

            if (!styles.TryGetValue((int)style, out ret))
            {
                ret = style == StandardStyle.Selection || style == StandardStyle.MatchedBracket || style == StandardStyle.MatchedWord
                        ? new SelectionStyle()
                    : style == StandardStyle.LineNumbers ? new MarginStyle()
                    : style == StandardStyle.ScrollBars ? new MarginStyle()
                    : style == StandardStyle.Folding ? new MarginStyle()
                    : style == StandardStyle.Popup ? new PopupStyle()
                    : style == StandardStyle.Caret ? new Style()
                    : style == StandardStyle.CurrentLine ? new Style()
                    : new TextStyle();

                Register((int)style, ret);
            }

            return ret;
        }

        public void Register(StyleId styleId, Style style) => Register(styleId, style);

        private void Register(int styleId, Style style)
        {
            var ts = style as TextStyle;

            if (ts != null)
            {
                ts.Cloned = ts.FullClone();
                ts.DefaultStyle = DefaultStyle;
                ts.Cloned.Default = false;
                ts.Cloned.DefaultStyle = DefaultStyle;
            }

            styles.Remove(styleId);
            styles.Add(styleId, style);
        }

        public TextStyle DefaultStyle { get; private set; }

        public int StyleCount => styles.Count;
    }
}
