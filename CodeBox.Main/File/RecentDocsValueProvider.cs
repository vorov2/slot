using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace CodeBox.Main.File
{
    [Export(typeof(IComponent))]
    [ComponentData("values.recentdocs")]
    public sealed class RecentDocsValueProvider : IArgumentValueProvider
    {
        [Import]
        private IViewManager viewManager = null;

        [Import]
        private IBufferManager bufferManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var cur = viewManager.GetActiveView().Buffer;
            return bufferManager.EnumerateBuffers()
                .Where(b => b != cur && (str == null || b.File.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1))
                .OrderByDescending(b => b.LastAccess)
                .Select(b => new ValueItem(b.File.Name, b.File.DirectoryName));
        }
    }
}
