using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core;

namespace CodeBox.BufferCommands
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
