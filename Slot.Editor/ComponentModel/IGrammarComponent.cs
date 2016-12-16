using Slot.Core.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace Slot.Editor.Lexing
{
    public interface IGrammarComponent : IComponent
    {
        IEnumerable<Grammar> EnumerateGrammars();

        Grammar GetGrammar(int id);

        Grammar GetGrammar(string key);

        Grammar GetGrammarByFile(FileInfo fi);
    }
}