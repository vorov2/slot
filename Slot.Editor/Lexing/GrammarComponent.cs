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
    internal sealed class GrammarInfo
    {
        public Identifier Key { get; set; }

        public Identifier Mode { get; set; }

        public FileInfo File { get; set; }
    }

    [Export(typeof(IGrammarComponent))]
    [ComponentData(Name)]
    public sealed class GrammarComponent : IGrammarComponent
    {
        public const string Name = "grammars.default";

        private readonly Dictionary<Identifier, Grammar> grammars = new Dictionary<Identifier, Grammar>();
        private readonly List<Grammar> index = new List<Grammar>();
        private readonly Dictionary<Identifier, GrammarInfo> infos = new Dictionary<Identifier, GrammarInfo>();

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
                    infos.Add(
                        key,
                        new GrammarInfo
                        {
                            Key = key,
                            Mode = (Identifier)e.String("mode"),
                            File = fi
                        });
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
                GrammarInfo inf;

                if (!infos.TryGetValue(key, out inf))
                    throw new SlotException($"Grammar '{key}' not found!");
                else
                {
                    string content;
                    if (!FileUtil.ReadFile(inf.File, Encoding.UTF8, out content))
                        throw new SlotException($"Unable to read grammar '{key}'!");

                    grammar = GrammarReader.Read(key, content);
                    grammars.Remove(inf.Key);
                    grammars.Add(inf.Key, grammar);
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
