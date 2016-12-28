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

        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return bufferManager.EnumerateRecent()
                .Select(b => new ValueItem(b.Name, b.DirectoryName));
        }
    }
}
