using CodeBox.Core.ComponentModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("app.file.open", "fo", ArgumentType = ArgumentType.String, ArgumentName = "fileName" )]
    public sealed class _TestCommand : ICommand, IArgumentValueProvider
    {
        public IEnumerable<ArgumentValue> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;

            if (!string.IsNullOrWhiteSpace(str))
                return GetPathElements(str) ?? Enumerable.Empty<ArgumentValue>();

            return Enumerable.Empty<ArgumentValue>();
        }

        private IEnumerable<ArgumentValue> GetPathElements(string pat)
        {
            if (pat.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return null;

            try
            {
                if (pat.EndsWith("\\") || pat.EndsWith("//"))
                    return Directory.EnumerateFileSystemEntries(pat)
                        .Where(v => v.StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new ArgumentValue { Value = sy });
                else
                {
                    var fi = new FileInfo(pat);

                    if (fi.Directory == null)
                        return null;

                    var loc = Environment.CurrentDirectory == fi.DirectoryName;
                    return fi.Directory.EnumerateFileSystemInfos()
                        .Where(v => (loc ? v.Name : v.FullName).StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new ArgumentValue { Value = sy.FullName });
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Run(IExecutionContext ctx, object arg)
        {
            if (arg == null)
                return false;

            var fn = arg.ToString();
            var txt = File.ReadAllText(fn, Encoding.UTF8);
            ((Editor)ctx).AttachBuffer(new DocumentBuffer(Document.Read(txt), fn, Encoding.UTF8));
            return true;
        }
    }
}
