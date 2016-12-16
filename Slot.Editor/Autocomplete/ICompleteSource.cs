using System;
using System.Collections.Generic;
using Slot.Core.ViewModel;

namespace Slot.Editor.Autocomplete
{
    public interface ICompleteSource
    {
        void Initialize(IView view);

        IEnumerable<string> GetItems();
    }
}
