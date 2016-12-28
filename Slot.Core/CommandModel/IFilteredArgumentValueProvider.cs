using System;
using System.Collections.Generic;

namespace Slot.Core.CommandModel
{
    public interface IFilteredArgumentValueProvider : IArgumentValueProvider
    {
        IEnumerable<ValueItem> EnumerateArgumentValues(string filter);
    }
}
