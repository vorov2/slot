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
        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return Encoding.GetEncodings()
                .Select(e => new ValueItem(e.Name, e.DisplayName))
                .Concat(new ValueItem[] { new ValueItem(Encoding.UTF8.WebName + "NB", $"Unicode (UTF-8 No BOM)") })
                .OrderBy(e => e.Value);
        }
    }
}
