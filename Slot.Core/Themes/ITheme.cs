using System;
using System.Collections.Generic;
using Slot.Core.ComponentModel;

namespace Slot.Core.Themes
{
    public interface ITheme : IComponent
    {
        IEnumerable<ThemeInfo> EnumerateThemes();

        Style GetStyle(StandardStyle styleId);

        bool ChangeTheme(Identifier theme);

        ThemeInfo Theme { get; }

        event EventHandler ThemeChanged;
    }
}
