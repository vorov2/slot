using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core;
using Slot.Core.Packages;
using Json;
using System.Text;

namespace Slot.Editor.Lexing
{
    [Export(typeof(IGrammarComponent))]
    [ComponentData(Name)]
    public sealed class GrammarComponent : IGrammarComponent
    {
        public const string Name = "grammars.default";

        private readonly Dictionary<Identifier, Grammar> grammars = new Dictionary<Identifier, Grammar>();
        private readonly List<Grammar> index = new List<Grammar>();
        private readonly Dictionary<Identifier, FileInfo> infos = new Dictionary<Identifier, FileInfo>();

        [Import]
        private IPackageManager packageManager = null;

        private void LoadGrammars()
        {
            if (infos.Count > 0)
                return;

            foreach (var pkg in packageManager.EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Grammars))
                {
                    FileInfo fi;

                    if (!FileUtil.TryGetInfo(Path.Combine(pkg.Directory.FullName, "data", e.String("file")), out fi))
                        continue;

                    var key = (Identifier)e.String("key");
                    infos.Add(key, fi);
                }
        }

        public Grammar GetGrammar(Identifier key)
        {
            if (key == null)
                return null;

            LoadGrammars();
            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
            {
                FileInfo inf;

                if (!infos.TryGetValue(key, out inf))
                    throw new SlotException($"Grammar '{key}' not found!");
                else
                {
                    string content;
                    if (!FileUtil.ReadFile(inf, Encoding.UTF8, out content))
                        throw new SlotException($"Unable to read grammar '{key}'!");

                    grammar = GrammarReader.Read(key, content);
                    grammars.Remove(key);
                    grammars.Add(key, grammar);
                    index.Add(grammar);
                    grammar.GlobalId = index.Count;
                }
            }

            return grammar;
        }

        public Grammar GetGrammar(int id)
        {
            LoadGrammars();
            return index[id - 1];
        }
    }
}
