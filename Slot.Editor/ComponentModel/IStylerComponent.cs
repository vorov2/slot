using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.ObjectModel;

namespace Slot.ComponentModel
{
    public interface IStylerComponent : IComponent
    {
        void Style(IView view, Range range);
    }
}
