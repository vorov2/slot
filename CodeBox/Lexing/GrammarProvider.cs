using System;
using System.Collections.Generic;

namespace CodeBox.Lexing
{
    public sealed class GrammarProvider
    {
        private readonly Dictionary<string, Grammar> grammars = new Dictionary<string, Grammar>();
        private readonly List<Grammar> index = new List<Grammar>();

        public void RegisterGrammar(Grammar grammar)
        {
            grammars.Remove(grammar.Key);
            grammars.Add(grammar.Key, grammar);
            index.Add(grammar);
            grammar.Id = index.Count;
        }

        public Grammar GetGrammar(string key)
        {
            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
                throw new CodeBoxException($"Grammar '{key}' not found!");

            return grammar;
        }

        internal Grammar GetGrammar(int id)
        {
            return index[id - 1];
        }
    }
}
