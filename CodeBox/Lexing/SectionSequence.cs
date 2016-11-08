using System;

namespace CodeBox.Lexing
{
    public sealed class SectionSequence : ISectionSequence
    {
        private readonly string sequence;
        private readonly bool caseSensitive;
        private MatchResult lastResult;

        public SectionSequence(string sequence, bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            this.sequence = !caseSensitive ? sequence.ToUpper() : sequence;
        }

        public MatchResult Match(char c)
        {
            if (c == '\t' || c == '\r' || c == '\n')
                c = ' ';

            if (lastResult == MatchResult.Hit)
                Reset();

            var sc = sequence.Length > Offset ? sequence[Offset] : '\0';
            var eq = caseSensitive ? sc == c : sc == char.ToUpper(c);

            if (eq && sc != ' ')
            {
                Offset++;

                if (Offset == sequence.Length)
                    return lastResult = MatchResult.Hit;
                else
                    return lastResult = MatchResult.Proc;
            }
            else if (sc == ' ' && sequence.Length > Offset + 1 && sequence[Offset + 1] == c)
            {
                Offset++;
                return lastResult = MatchResult.Hit;
            }
            else if (sc == ' ')
            {
                return lastResult = MatchResult.Proc;
            }
            else
            {
                Reset();
                return lastResult = MatchResult.Fail;
            }
        }

        public MatchResult TryMatch(char c, int shift = 0)
        {
            var os = Offset;
            var oldret = lastResult;
            Offset += shift;
            var ret = MatchResult.Fail;

            if (Offset < sequence.Length && MatchAnyState)
                ret = MatchResult.Hit;
            else
                ret = Match(c);

            Offset = os;
            lastResult = oldret;
            return ret;
        }

        public bool MatchAnyState => sequence[Offset] == ' ';

        public char First() => sequence[0];

        public void Reset() => Offset = 0;

        public int Offset { get; private set; }

        public int Length => sequence.Length;
    }
}
