using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace CodeBox.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.systempath")]
    public class SystemPathValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return GetPathElements(str) ?? Enumerable.Empty<ValueItem>();
        }

        private IEnumerable<ValueItem> GetPathElements(string pat)
        {
            pat = string.IsNullOrWhiteSpace(pat) ? null : pat;

            if (pat != null && pat.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return null;

            try
            {
                var cur = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
                var file = pat != null ? new FileInfo(pat) : null;
                var path = file != null && (file.Directory != null && file.Directory.Exists) ? file.DirectoryName
                    : pat != null && pat.EndsWith(Path.DirectorySeparatorChar.ToString()) ? pat
                    : cur;

                var qry = Directory.EnumerateDirectories(path)
                    .Select(d => d + Path.DirectorySeparatorChar);

                if (IncludeFiles)
                    qry = qry.Concat(Directory.EnumerateFiles(path));

                return qry
                    .Select(fi => fi.Replace(cur, ""))
                    .Where(fi => pat == null || fi.StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                    .Select(fi => new ValueItem(fi));
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual bool IncludeFiles => true;
    }
}
