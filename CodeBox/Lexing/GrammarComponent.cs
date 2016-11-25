using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Lexing
{
    [Export(typeof(IComponent))]
    [ComponentData("grammar.default")]
    public sealed class GrammarComponent : IGrammarComponent
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

        public IEnumerable<Grammar> EnumerateGrammars()
        {
            return index;
        }
 
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

        public Grammar GetGrammar(int id)
        {
            return index[id - 1];
        }
    }
}
