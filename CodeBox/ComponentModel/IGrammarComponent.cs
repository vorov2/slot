using CodeBox.Core.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace CodeBox.Lexing
{
    public interface IGrammarComponent : IComponent
    {
        IEnumerable<Grammar> EnumerateGrammars();

        Grammar GetGrammar(int id);

        Grammar GetGrammar(string key);

        Grammar GetGrammarByFile(FileInfo fi);

        void RegisterGrammar(Grammar grammar);
    }
}