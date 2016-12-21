using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Slot.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.encoding")]
    public sealed class EncodingValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return Encoding.GetEncodings()
                .Where(e => str == null || e.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(e => new ValueItem(e.Name, e.DisplayName))
                .Concat(new ValueItem[] { new ValueItem(Encoding.UTF8.WebName + " no bom", Encoding.UTF8.EncodingName) })
                .OrderBy(e => e.Value);
        }
    }
}
