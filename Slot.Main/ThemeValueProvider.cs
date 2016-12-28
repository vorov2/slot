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
        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return App.Component<ITheme>().EnumerateThemes()
                .Select(t => new ValueItem(t.Key.ToString(), t.Name));
        }
    }
}
