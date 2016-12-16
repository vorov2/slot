using System;
using Slot.Core.ComponentModel;
using Slot.Editor.ObjectModel;
using Slot.Core.ViewModel;

namespace Slot.Editor.ComponentModel
{
    public interface IFoldingComponent : IComponent
    {
        void Fold(IView view, Range range);
    }
}
