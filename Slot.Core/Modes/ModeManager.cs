using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Json;
using Slot.Core.ComponentModel;
using Slot.Core.Packages;

namespace Slot.Core.Modes
{
    [Export(typeof(IModeManager))]
    [ComponentData(Name)]
    public sealed class ModeManager : IModeManager
    {
        public const string Name = "modes.default";
        private volatile bool loaded;
        private readonly List<ModeMetadata> modes = new List<ModeMetadata>();

        [Import]
        private IPackageManager packageManager = null;

        private void EnsureLoad()
        {
            if (loaded)
                return;

            foreach (var pkg in packageManager.EnumeratePackages())
                foreach (var m in pkg.GetMetadata(PackageSection.Modes))
                    modes.Add(new ModeMetadata(
                        (Identifier)m.String("key"), m.String("name"),
                        m.Enum<ModeKind>("kind"), m.List<string>("extensions")));
            loaded = true;
        }

        public IEnumerable<ModeMetadata> EnumerateModes()
        {
            EnsureLoad();
            return modes;
        }

        public ModeMetadata GetMode(Identifier key)
        {
            var ret = modes.FirstOrDefault(m => m.Key == key);

            if (ret == null)
                throw new SlotException($"Unknown app mode: {key}!");

            return ret;
        }

        public ModeMetadata SelectMode(FileInfo file)
        {
            EnsureLoad();
            var ret = Select(file);

            if (ret == null)
                throw new SlotException($"Unable to find an app mode for file: {file}");

            return ret;
        }

        private ModeMetadata Select(FileInfo file)
        {
            var find = modes.Where(m => m.Match(file));

            if (!find.Any())
                return modes.FirstOrDefault(m => m.Kind == ModeKind.Default);
            else if (find.Count() > 1)
            {
                var ret = find.FirstOrDefault(m => m.Kind == ModeKind.Suppressing);
                return ret ?? find.First();
            }
            else
                return find.First();
        }
    }
}
