using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core.Themes;
using Slot.Core.ViewModel;

namespace Slot.Main.Theme
{
    [Export(typeof(IThemeManager))]
    [ComponentData(Name)]
    public sealed class ThemeManager : IThemeManager
    {
        public const string Name = "themes.default";

        public ITheme Create(IView view)
        {
            return new RealTheme(view);
        }
    }
}
