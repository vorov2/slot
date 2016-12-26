using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Slot.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.recentdocs")]
    public sealed class RecentDocsValueProvider : IArgumentValueProvider
    {
        [Import]
        private IBufferManager bufferManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return bufferManager.EnumerateRecent()
                .Where(b => str == null || b.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(b => new ValueItem(b.Name, b.DirectoryName));
        }
    }
}
