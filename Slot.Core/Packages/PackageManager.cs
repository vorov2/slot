using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;
using Slot.Core.ComponentModel;
using Slot.Core.Output;

namespace Slot.Core.Packages
{
    [Export(typeof(IPackageManager))]
    [ComponentData(Name)]
    public sealed class PackageManager : IPackageManager
    {
        public const string Name = "packages.default";
        private volatile bool loaded;
        private readonly List<PackageMetadata> packages = new List<PackageMetadata>();

        [Import("directory.packages")]
        private string packagesDirectory = null;

        private void EnsureLoad()
        {
            if (loaded)
                return;

            foreach (var d in Directory.GetDirectories(packagesDirectory))
            {
                var fi = new FileInfo(Path.Combine(d, "package.json"));

                if (!fi.Exists)
                {
                    App.Ext.Log($"Missing package definition file at: {d}", EntryType.Error);
                    continue;
                }

                string content;

                if (!FileUtil.ReadFile(fi, Encoding.UTF8, out content))
                    continue;

                var json = new JsonParser(content);
                var dict = json.Parse() as Dictionary<string, object>;

                if (dict == null)
                {
                    App.Ext.Log($"Invalid package definition file: {fi}", EntryType.Error);
                    continue;
                }

                var meta = dict.Object("meta") as Dictionary<string, object>;
                var pack = new PackageMetadata((Identifier)meta.String("key"),
                    meta.String("name"), meta.String("version"), meta.String("description"),
                    meta.String("copyright"), fi.Directory, dict);
                packages.Add(pack);
            }

            loaded = true;
        }

        public IEnumerable<PackageMetadata> EnumeratePackages()
        {
            EnsureLoad();
            return packages;
        }
    }
}
