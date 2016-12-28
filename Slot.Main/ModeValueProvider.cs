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
        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return App.Component<IModeManager>().EnumerateModes()
                .Select(g => new ValueItem(g.Key.ToString(), g.Name));
        }
    }
}
