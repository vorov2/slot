using System;
using Slot.Core.ComponentModel;
using System.Collections.Generic;

namespace Slot.Core.CommandModel
{
    public interface IArgumentValueProvider : IComponent
    {
        IEnumerable<ValueItem> EnumerateArgumentValues();
    }
}
