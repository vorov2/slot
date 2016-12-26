using Slot.Core.ComponentModel;
using System;
using System.Collections.Generic;

namespace Slot.Core.ViewModel
{
    public interface IViewManager : IComponent
    {
        IView CreateView();

        void CloseView(IView view);

        IView GetActiveView();

        void ActivateView(IView view);

        IEnumerable<IView> EnumerateViews();
    }
}
