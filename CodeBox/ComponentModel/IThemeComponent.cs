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
        void ChangeTheme(string themeKey);

        Style GetStyle(int styleId);

        Style GetStyle(StandardStyle style);

        Style DefaultStyle { get; }
    }
}
