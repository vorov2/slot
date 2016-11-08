using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public static class EnumerableExtensions
    {
        public static GrammarSection TryMatch(this IEnumerable<GrammarSection> sections, Line ln, int col, GrammarSection except)
        {
            foreach (var sect in sections)
            {
                if (sect == except || sect.Start == null)
                    continue;

                var res = sect.Start.TryMatch(ln.CharAt(col + 0), 0);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 1), 1);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 2), 2);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 3), 3);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 4), 4);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 5), 5);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 6), 6);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 7), 7);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 8), 8);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 9), 9);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;
            }

            return null;
        }

        public static GrammarSection MatchByKeyword(this IEnumerable<GrammarSection> sections, int key)
        {
            foreach (var sect in sections)
            {
                if (sect.StartKeyword == key)
                    return sect;
            }

            return null;
        }

        public static GrammarSection Match(this IEnumerable<GrammarSection> sections, char c)
        {
            var res = MatchResult.Fail;
            var match = default(GrammarSection);

            foreach (var sect in sections)
            {
                if (sect.Start == null)
                    continue;

                res = sect.Start.Match(c);

                if (res == MatchResult.Hit && match == null)
                    match = sect;
                else if (res == MatchResult.Hit
                    && sect.Start.Offset > match.Start.Offset)
                    match = sect;
            }

            return match;
        }

        public static void Reset(this IEnumerable<GrammarSection> sections)
        {
            foreach (var sect in sections)
                if (sect.Start != null)
                    sect.Start.Reset();
        }
    }
}
