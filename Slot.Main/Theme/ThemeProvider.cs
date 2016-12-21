using Slot.Core;
using Slot.Core.ComponentModel;
using Slot.Core.Themes;
using Slot.Core.ViewModel;
using Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Slot.Core.Packages;
using Slot.Core.Settings;
using System.Windows.Forms;
using System.Text;

namespace Slot.Main.Theme
{
    [Export(typeof(IThemeComponent))]
    [ComponentData(Name)]
    public sealed class ThemeProvider : IThemeComponent
    {
        public const string Name = "theme.default";
        private readonly Dictionary<Identifier, ThemeInfo> themes = new Dictionary<Identifier, ThemeInfo>();
        private readonly Dictionary<StandardStyle, Style> styles = new Dictionary<StandardStyle, Style>();
        
        [Import]
        private IPackageManager packageManager = null;

        [Import]
        private IViewManager viewManager = null;

        public IEnumerable<ThemeInfo> EnumerateThemes()
        {
            return themes.Values;
        }

        private bool ChangeTheme(Identifier themeKey)
        {
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

                    foreach (var v in viewManager.EnumerateViews().OfType<Control>())
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

            foreach (var pkg in packageManager.EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Themes))
                    themes.Add(
                        (Identifier)e.String("key"),
                        new ThemeInfo(
                            (Identifier)e.String("key"),
                            e.String("name"),
                            new FileInfo(Path.Combine(pkg.Directory.FullName, "data", e.String("file")))
                        ));

            var set = App.Catalog<ISettingsProvider>().Default().Get<EnvironmentSettings>();
            set.SettingsChanged += (o, ev) =>
            {
                var kt = (Identifier)set.Theme;
                if (Theme?.Key != kt)
                    ChangeTheme(kt);
            };
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
