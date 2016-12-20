using Slot.Core.ComponentModel;
using System.Collections.Generic;
using System.IO;
using Slot.Core;

namespace Slot.Editor.Lexing
{
    public interface IGrammarComponent : IComponent
    {
        Grammar GetGrammar(int id);

        Grammar GetGrammar(Identifier key);
    }
}