using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace CodeBox.Styling
{
    [Export(typeof(IComponent))]
    [ComponentData("theme.default")]
    public sealed class ThemeComponent : IThemeComponent
    {
        private readonly Dictionary<int, Style> styles = new Dictionary<int, Style>();

        [Import("directory.theme")]
        private string themePath = null;

        [Import("directory.root")]
        private string rootPath = null;

        public ThemeComponent()
        {
            var def = new TextStyle { Default = true };
            Register(0, def);
            DefaultStyle = def;
        }

        public void ChangeTheme(string themeKey)
        {
            var dir = new DirectoryInfo(Path.Combine(rootPath, themePath));
            var fi = dir.EnumerateFiles($"{themeKey}.theme.json").FirstOrDefault();

            if (fi != null)
                StylesReader.Read(File.ReadAllText(fi.FullName), this);
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

        public Style DefaultStyle { get; private set; }
    }
}
