using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Affinity
{
    public sealed class NumberLiteral
    {
        private readonly List<Sect> pattern;
        private int pointer;
        private const string BRACKETS1 = "({[";
        private const string BRACKETS2 = ")}]";
        private const string DELIMS = " \0\t(){}[]<>,;~!?-+%#^&*/\\='\"";

        public class Sect
        {
            public string Pattern;
            public bool Multiple;
            public bool Optional;

            public override string ToString()
            {
                return string.Format((Multiple ? "{{{0}}}" : "({0})"), Pattern);
            }
        }

        public NumberLiteral(string pattern)
        {
            this.pattern = ParsePattern(pattern.ToUpper());
        }

        public bool Match(char c, char last)
        {
            if (pointer >= pattern.Count || (MatchCount == 0 && DELIMS.IndexOf(last) == -1))
            {
                Reset();
                return false;
            }

            c = char.ToUpper(c);
            var par = pattern[pointer];
            var has = par.Pattern.IndexOf(c) != -1;

            //Special case
            if (has && (c == '+' || c == '-'))
                has = last == 'e' || last == 'E';

            if (!has && (par.Multiple || par.Optional))
            {
                pointer++;
                return Match(c, last);
            }

            if (has && !par.Multiple)
                pointer++;

            if (has)
                MatchCount++;
            else
                Reset();

            return has;
        }

        private void Reset()
        {
            MatchCount = 0;
            pointer = 0;
        }

        private List<Sect> ParsePattern(string pattern)
        {
            var start = -1;
            var seq = new List<Sect>();
            var buffer = pattern.ToCharArray();
            var expect = '\0';

            for (var i = 0; i < buffer.Length; i++)
            {
                var c = buffer[i];
                var idx = -1;

                if ((idx = BRACKETS1.IndexOf(c)) != -1 && start == -1)
                {
                    expect = BRACKETS2[idx];
                    start = i;
                }
                else if (c == expect && start != -1)
                {
                    seq.Add(new Sect
                    {
                        Pattern = new string(buffer, start + 1, i - start - 1),
                        Multiple = c == '}',
                        Optional = c == ']'
                    });
                    start = -1;
                }
            }

            return seq;
        }

        public int MatchCount { get; private set; }
    }
}
