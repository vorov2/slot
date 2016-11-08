using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Grammars
{
    public interface IGrammar
    {
        string NonWordSymbols { get; }
    }
}
