using CodeBox.Core.ComponentModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ComponentModel
{
    public interface IThemeComponent : IComponent
    {
        IEnumerable<ThemeInfo> EnumerateThemes();

        void ChangeTheme(string themeKey);

        Style GetStyle(int styleId);

        Style GetStyle(StandardStyle style);

        Style DefaultStyle { get; }
    }

    public sealed class ThemeInfo
    {
        public string Key { get; internal set; }

        public string Name { get; internal set; }

        public string File { get; internal set; }
    }
}
