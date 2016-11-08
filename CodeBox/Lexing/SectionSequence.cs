using System;

namespace CodeBox.Lexing
{
    public sealed class SectionSequence
    {
        private readonly string sequence;
        private readonly bool caseSensitive;

        public SectionSequence(string sequence, bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            this.sequence = !caseSensitive ? sequence.ToUpper() : sequence;
        }

        internal MatchResult Match(char c)
        {
            if (c == '\t' || c == '\r' || c == '\n')
                c = ' ';

            if (LastResult == MatchResult.Hit)
                Reset();

            var sc = sequence.Length > Offset ? sequence[Offset] : '\0';
            var eq = caseSensitive ? sc == c : sc == char.ToUpper(c);

            if (eq && sc != ' ')
            {
                Offset++;

                if (Offset == sequence.Length)
                    return LastResult = MatchResult.Hit;
                else
                    return LastResult = MatchResult.Proc;
            }
            else if (sc == ' ' && sequence.Length > Offset + 1 && sequence[Offset + 1] == c)
            {
                Offset++;
                return LastResult = MatchResult.Hit;
            }
            else if (sc == ' ')
            {
                return LastResult = MatchResult.Proc;
            }
            else
            {
                Reset();
                return LastResult = MatchResult.Fail;
            }
        }

        internal MatchResult TryMatch(char c, int shift = 0)
        {
            var os = Offset;
            var oldret = LastResult;
            Offset += shift;
            var ret = MatchResult.Fail;

            if (Offset < sequence.Length && MatchAnyState)
                ret = MatchResult.Hit;
            else
                ret = Match(c);

            Offset = os;
            LastResult = oldret;
            return ret;
        }

        public bool MatchAnyState => sequence[Offset] == ' ';

        public char First() => sequence[0];

        public void Reset() => Offset = 0;

        internal int Offset { get; private set; }

        internal int Length => sequence.Length;

        internal MatchResult LastResult { get; private set; }
    }
}
