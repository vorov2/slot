using Slot.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core.CommandModel
{
    public interface IArgumentValueProvider : IComponent
    {
        IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue);
    }
}
