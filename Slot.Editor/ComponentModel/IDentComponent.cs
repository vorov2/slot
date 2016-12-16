using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Editor.ComponentModel
{
    public interface IDentComponent : IComponent
    {
        int CalculateIndentation(IView view, int lineIndex);
    }
}
