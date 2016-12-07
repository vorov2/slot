using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Lexing
{
    [Export(typeof(IGrammarComponent))]
    [ComponentData(Name)]
    public sealed class GrammarComponent : IGrammarComponent
    {
        public const string Name = "grammar.default";

        private readonly Dictionary<string, Grammar> grammars = new Dictionary<string, Grammar>();
        private readonly List<Grammar> index = new List<Grammar>();

        [Import("directory.grammar")]
        private string grammarPath = null;

        [Import("directory.root")]
        private string rootPath = null;

        private void LoadGrammars()
        {
            if (index.Count > 0)
                return;

            var dir = new DirectoryInfo(Path.Combine(rootPath, grammarPath));
            
            foreach (var fi in dir.GetFiles("*.grammar.json"))
            {
                var grm = GrammarReader.Read(File.ReadAllText(fi.FullName));
                RegisterGrammar(grm);
            }
        }

        private void RegisterGrammar(Grammar grammar)
        {
            grammars.Remove(grammar.Key);
            grammars.Add(grammar.Key, grammar);
            index.Add(grammar);
            grammar.GlobalId = index.Count;
        }

        public IEnumerable<Grammar> EnumerateGrammars()
        {
            LoadGrammars();
            return index;
        }
 
        public Grammar GetGrammar(string key)
        {
            if (key == null)
                return null;

            LoadGrammars();
            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
                throw new CodeBoxException($"Grammar '{key}' not found!");

            return grammar;
        }

        public Grammar GetGrammarByFile(FileInfo fi)
        {
            LoadGrammars();
            var ext = fi.Extension.TrimStart('.');
            var ret = grammars
                .FirstOrDefault(g => g.Value.Extensions.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase)));
            return ret.Value;
        }

        public Grammar GetGrammar(int id)
        {
            LoadGrammars();
            return index[id - 1];
        }
    }
}
