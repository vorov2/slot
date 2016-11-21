using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace CodeBox.Lexing
{
    public sealed class GrammarManager
    {
        private readonly Dictionary<string, Grammar> grammars = new Dictionary<string, Grammar>();
        private readonly List<Grammar> index = new List<Grammar>();

        public void RegisterGrammar(Grammar grammar)
        {
            grammars.Remove(grammar.Key);
            grammars.Add(grammar.Key, grammar);
            index.Add(grammar);
            grammar.GlobalId = index.Count;
        }

        public Grammar GetRootGrammar() => GetGrammar(GrammarKey);

        public Grammar GetGrammar(string key)
        {
            if (key == null)
                return null;

            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
                throw new CodeBoxException($"Grammar '{key}' not found!");

            return grammar;
        }

        public Grammar GetGrammarByFile(FileInfo fi)
        {
            var ext = fi.Extension.TrimStart('.');
            var ret = grammars
                .FirstOrDefault(g => g.Value.Extensions.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase)));
            return ret.Value;
        }

        internal Grammar GetGrammar(int id)
        {
            return index[id - 1];
        }

        public string GrammarKey { get; set; }
    }
}
