using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.infos")]
    public sealed class InfoValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = (curvalue ?? "").ToString();
            return Enums.GetDisplayNames<EnvironmentCommandDispatcher.Info>()
                .Where(s => s.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(s => new ValueItem(s));
        }
    }
}
