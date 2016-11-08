using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public sealed class ChoiceSectionSequence : ISectionSequence
    {
        private char fst;
        private char snd;
        private bool caseSensitive;

        public ChoiceSectionSequence(char fst, char snd, bool caseSensitive)
        {
            this.fst = caseSensitive ? fst : char.ToUpper(fst);
            this.snd = caseSensitive ? snd : char.ToUpper(snd);
            this.caseSensitive = caseSensitive;
        }

        public MatchResult Match(char c)
        {
            if (Offset > 0)
            {
                Reset();
                return MatchResult.Fail;
            }

            c = caseSensitive ? c : char.ToUpper(c);

            if (c == fst || c == snd)
            {
                Offset++;
                return MatchResult.Hit;
            }
            else
            {
                Reset();
                return MatchResult.Fail;
            }
        }

        public MatchResult TryMatch(char c, int shift = 0)
        {
            return Offset + shift == 0 && (c == fst || c == snd) ?
                MatchResult.Hit : MatchResult.Fail;
        }

        public char First() => fst;

        public void Reset() => Offset = 0;

        public int Length => 1;

        public bool MatchAnyState => false;

        public int Offset { get; private set; }
    }
}
