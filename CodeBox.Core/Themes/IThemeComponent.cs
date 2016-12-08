using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.Themes
{
    public interface IThemeComponent : IComponent
    {
        IEnumerable<ThemeInfo> EnumerateThemes();

        bool ChangeTheme(Identifier themeKey);

        Style GetStyle(StandardStyle styleId);

        ThemeInfo Theme { get; }

        event EventHandler ThemeChanged;
    }
}
