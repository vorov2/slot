using System;
using System.Collections.Generic;

namespace CodeBox.Lexing
{
    public sealed class GrammarProvider
    {
        private readonly Dictionary<string, Grammar> grammars = new Dictionary<string, Grammar>();

        public void RegisterGrammar(Grammar grammar)
        {
            grammars.Remove(grammar.Key);
            grammars.Add(grammar.Key, grammar);
        }

        public Grammar GetGrammar(string key)
        {
            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
                throw new CodeBoxException($"Grammar '{key}' not found!");

            return grammar;
        }
    }
}
