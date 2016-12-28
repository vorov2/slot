using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Slot.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.commands")]
    public sealed class CommandsValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return App.Component<ICommandProvider>().EnumerateCommands()
                .Where(c => c.Title != null && c.Alias != "?")
                .Select(c => new CommandArgumentValue(c));
        }

        class CommandArgumentValue : ValueItem
        {
            private readonly CommandMetadata meta;

            internal CommandArgumentValue(CommandMetadata meta)
            {
                this.meta = meta;
            }

            public override string Value => meta.Title;

            public override string Meta =>
                meta.Shortcut != null ? $"{meta.Shortcut} ({meta.Alias})" : $"{meta.Alias}";

            public override string ToString() => Value;
        }
    }
}
