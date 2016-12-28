using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData(Name)]
    public sealed class SettingsValueProvider : IArgumentValueProvider
    {
        public const string Name = "values.settings";

        private static readonly ValueItem[] items =
        {
            new ValueItem("global", "Global settings (read only)"),
            new ValueItem("user", "User settings (override global)"),
            new ValueItem("workspace", "Workspace settings (override user)"),
        };

        public IEnumerable<ValueItem> EnumerateArgumentValues() => items;
    }
}
