using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;

namespace CodeBox.BufferCommands
{
    [Export(typeof(IComponent))]
    [ComponentData("values.eol")]
    public sealed class EolValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return Enums.GetDisplayNames<Eol>()
                .Where(e => str == null || e.Contains(str))
                .Select(e => new ValueItem(e));
        }
    }
}
