using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Themes;
using CodeBox.Core.ViewModel;
using Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace CodeBox.Styling
{
    [Export(typeof(IThemeComponent))]
    [ComponentData(Name)]
    public sealed class ThemeProvider : IThemeComponent
    {
        public const string Name = "theme.default";
        private readonly Dictionary<Identifier, ThemeInfo> themes = new Dictionary<Identifier, ThemeInfo>();
        private readonly Dictionary<StandardStyle, Style> styles = new Dictionary<StandardStyle, Style>();

        [Import("directory.theme")]
        private string themePath = null;

        [Import("directory.root")]
        private string rootPath = null;

        [Import]
        private IViewManager viewManager = null;

        public IEnumerable<ThemeInfo> EnumerateThemes()
        {
            return themes.Values;
        }

        public bool ChangeTheme(Identifier themeKey)
        {
            ReadThemes();
            ThemeInfo th;

            if (themes.TryGetValue(themeKey, out th))
            {
                if (th.File.Exists)
                {
                    foreach (var s in ThemeReader.Read(File.ReadAllText(th.File.FullName)))
                        Register(s.StyleId, s.Style);

                    foreach (var v in viewManager.EnumerateViews().OfType<Editor>())
                        v.Redraw();
                }

                Theme = th;
                OnThemeChanged();
                return true;
            }

            return false;
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
                        e => themes.Add((Identifier)e.String("key"),
                            new ThemeInfo((Identifier)e.String("key"), e.String("name"),
                                new FileInfo(Path.Combine(rootPath, themePath, e.String("file")))
                            )));
            }
        }

        public Style GetStyle(StandardStyle style)
        {
            Style ret;

            if (!styles.TryGetValue(style, out ret))
            {
                ret = new Style();
                Register(style, ret);
            }

            return ret;
        }

        private void Register(StandardStyle styleId, Style style)
        {
            styles.Remove(styleId);
            styles.Add(styleId, style);
        }

        public ThemeInfo Theme { get; private set; }

        public event EventHandler ThemeChanged;
        private void OnThemeChanged() => ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
