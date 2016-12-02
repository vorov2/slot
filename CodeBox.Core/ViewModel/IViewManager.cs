using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;

namespace CodeBox.Core.ViewModel
{
    public interface IViewManager : IComponent
    {
        IView CreateView();

        IView GetActiveView();

        void ActivateView(IView view);

        IEnumerable<IView> EnumerateViews();
    }
}
