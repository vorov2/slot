using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Themes;
using Slot.Core.ViewModel;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.themes")]
    public sealed class ThemeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var theme = App.Catalog<IViewManager>().Default().GetActiveView().Theme;
            return theme.EnumerateThemes()
                .Where(t => str == null || t.Key.ToString().IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(t => new ValueItem(t.Key.ToString(), t.Name));
        }
    }
}
