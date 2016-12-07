using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace CodeBox.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.modifieddocs")]
    public sealed class ModifiedDocsValueProvider : IArgumentValueProvider
    {
        [Import]
        private IBufferManager bufferManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return bufferManager.EnumerateBuffers()
                .OfType<IMaterialBuffer>()
                .Where(b => b.IsDirty && (str == null || b.File.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1))
                .OrderByDescending(b => b.LastAccess)
                .Select(b => new ValueItem(b.File.Name, b.File.DirectoryName));
        }
    }
}
