using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public interface ISectionSequence
    {
        MatchResult Match(char c);

        MatchResult TryMatch(char c, int shift = 0);

        bool MatchAnyState { get; }

        char First();

        void Reset();

        int Length { get; }

        int Offset { get; }
    }
}
