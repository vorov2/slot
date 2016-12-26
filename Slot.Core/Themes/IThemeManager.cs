using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.Themes
{
    public interface IThemeManager : IComponent
    {
        ITheme Create(IView view);
    }
}
