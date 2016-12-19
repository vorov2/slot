using System;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    public interface ICommandBar : IComponent
    {
        void Show(IView view);

        void Show(IView view, string commandAlias, params object[] args);

        void ToggleMessage();

        void Hide(IView view);
    }
}
