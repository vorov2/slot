using CodeBox.ComponentModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace CodeBox.Styling
{
    [Export(typeof(IComponent))]
    [ComponentData(Name)]
    public sealed class ThemeComponent : IThemeComponent
    {
        public const string Name = "theme.default";
        private readonly Dictionary<string, ThemeInfo> themes = new Dictionary<string, ThemeInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, Style> styles = new Dictionary<int, Style>();

        [Import("directory.theme")]
        private string themePath = null;

        [Import("directory.root")]
        private string rootPath = null;

        [Import]
        private IViewManager viewManager = null;

        public ThemeComponent()
        {
            var def = new TextStyle { Default = true };
            Register(0, def);
            DefaultStyle = def;
        }

        public IEnumerable<ThemeInfo> EnumerateThemes()
        {
            return themes.Values;
        }

        public void ChangeTheme(string themeKey)
        {
            ReadThemes();

            ThemeInfo th;

            if (themes.TryGetValue(themeKey, out th))
            {
                var fi = new FileInfo(Path.Combine(rootPath, themePath, th.File));

                if (fi.Exists)
                {
                    ThemeReader.Read(File.ReadAllText(fi.FullName), this);

                    foreach (var v in viewManager.EnumerateViews().OfType<Editor>())
                        v.Redraw();
                }
            }
        }

        private void ReadThemes()
        {
            if (themes.Count > 0)
                return;

            var fi = new FileInfo(Path.Combine(rootPath, themePath, "themes.json"));

            if (fi.Exists)
            {
                var json = new JsonParser(File.ReadAllText(fi.FullName));
                var list = json.Parse() as List<object>;

                if (list != null)
                    list.OfType<Dictionary<string, object>>()
                        .ToList()
                        .ForEach(
                        e => themes.Add(e.String("key"),
                        new ThemeInfo
                        {
                            Key = e.String("key"),
                            Name = e.String("name"),
                            File = e.String("file")
                        }));
            }
        }

        public Style GetStyle(int styleId) => styles[styleId];

        public Style GetStyle(StandardStyle style)
        {
            Style ret;

            if (!styles.TryGetValue((int)style, out ret))
            {
                ret = style == StandardStyle.Selection || style == StandardStyle.MatchedBracket || style == StandardStyle.MatchedWord
                        ? new SelectionStyle()
                    : style == StandardStyle.CommandBar ? new CommandBarStyle()
                    : style == StandardStyle.LineNumbers ? new MarginStyle()
                    : style == StandardStyle.ScrollBars ? new MarginStyle()
                    : style == StandardStyle.Folding ? new MarginStyle()
                    : style == StandardStyle.StatusBar ? new MarginStyle()
                    : style == StandardStyle.Popup ? new PopupStyle()
                    : style == StandardStyle.Caret ? new Style()
                    : style == StandardStyle.CurrentLine ? new Style()
                    : style == StandardStyle.SearchItem ? new SearchItemStyle()
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
                ts.Cloned.Default = ts.Default;
                ts.Cloned.DefaultStyle = DefaultStyle;
            }

            styles.Remove(styleId);
            styles.Add(styleId, style);
        }

        public Style DefaultStyle { get; private set; }
    }
}
