using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core;
using Slot.Editor;
using Slot.Core.Modes;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.modes")]
    public sealed class ModeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = (curvalue ?? "").ToString();
            return App.Catalog<IModeManager>().Default().EnumerateModes()
                .Where(g => g.Key.ToString().IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(g => new ValueItem(g.Key.ToString(), g.Name));
        }
    }
}
