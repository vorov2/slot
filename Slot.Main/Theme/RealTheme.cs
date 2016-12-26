using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Json;
using Slot.Core;
using Slot.Core.Packages;
using Slot.Core.Themes;
using Slot.Core.ViewModel;

namespace Slot.Main.Theme
{
    internal sealed class RealTheme : ITheme
    {
        private readonly Dictionary<Identifier, ThemeInfo> themes = new Dictionary<Identifier, ThemeInfo>();
        private readonly Dictionary<StandardStyle, Style> styles = new Dictionary<StandardStyle, Style>();
        private readonly IView view;

        public RealTheme(IView view)
        {
            this.view = view;
        }

        public IEnumerable<ThemeInfo> EnumerateThemes()
        {
            ReadThemes();
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
                    string content;

                    if (!FileUtil.ReadFile(th.File, Encoding.UTF8, out content))
                        return false;

                    foreach (var s in ThemeReader.Read(content))
                        Register(s.StyleId, s.Style);

                    foreach (var v in App.Catalog<IViewManager>().Default().EnumerateViews().OfType<Control>())
                        v.Invalidate(true);
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

            foreach (var pkg in App.Catalog<IPackageManager>().Default().EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Themes))
                    themes.Add(
                        (Identifier)e.String("key"),
                        new ThemeInfo(
                            (Identifier)e.String("key"),
                            e.String("name"),
                            new FileInfo(Path.Combine(pkg.Directory.FullName, "data", e.String("file")))
                        ));

            var set = view.Settings.Get<EnvironmentSettings>();
            ChangeTheme((Identifier)set.Theme);
        }

        public Style GetStyle(StandardStyle style)
        {
            ReadThemes();
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
