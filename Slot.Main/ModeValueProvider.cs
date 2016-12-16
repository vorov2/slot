using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core;
using Slot.Editor;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.modes")]
    public sealed class ModeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = (curvalue ?? "").ToString();
            return App.Ext.Grammars().EnumerateGrammars()
                .Where(g => g.Key.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(g => new ValueItem(g.Key, g.Name));
        }
    }
}
