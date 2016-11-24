using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core.CommandModel
{
    public interface IArgumentValueProvider : IComponent
    {
        IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue);
    }
}
