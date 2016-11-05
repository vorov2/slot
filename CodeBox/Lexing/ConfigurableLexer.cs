using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public sealed class ConfigurableLexer : IStylingProvider
    {
        public ConfigurableLexer()
        {
            Sections = new List<GrammarSection>();
            AddSection(new GrammarSection {
                Id = 0,
                Style = 0
            });
            AddSection(new GrammarSection {
                Id = 1,
                Start = new SectionSequence("<script >", false),
                End = new SectionSequence("</script>", false),
                Multiline = true,
                Style = 112 });
            AddSection(new GrammarSection
            {
                Id = 2,
                Start = new SectionSequence("//", true),
                StyleCompletely = true,
                Style = 112
            });
            AddSection(new GrammarSection
            {
                Id = 3,
                Start = new SectionSequence("/*", true),
                End = new SectionSequence("*/", true),
                Multiline = true,
                StyleCompletely = true,
                Style = 112
            });
            AddSection(new GrammarSection
            {
                Id = 4,
                Start = new SectionSequence("'", true),
                End = new SectionSequence("'", true),
                StyleCompletely = true,
                Style = 111
            });
            AddSection(new GrammarSection
            {
                Id = 5,
                Start = new SectionSequence("\"", true),
                End = new SectionSequence("\"", true),
                StyleCompletely = true,
                Style = 111
            });
            AddSection(new GrammarSection
            {
                Id = 6,
                Start = new SectionSequence("\"\"\"", true),
                End = new SectionSequence("\"\"\"", true),
                Multiline = true,
                StyleCompletely = true,
                Style = 112
            });
            Sections[0].Keywords.AddRange("o'brain get set in new volatile override virtual using namespace readonly static const public private internal protected sealed class struct abstract void int byte long short sbyte uint ushort ulong string char bool object var if else return while for foreach continue break ref out", 110);
        }

        private void AddSection(GrammarSection section)
        {
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
        }

        public void Style(IEditorContext context, Range range)
        {
            Context = context;
            Parse(new State(Lines[range.Start.Line].State), range);
        }

        private void Parse(State state, Range rng)
        {
            Console.WriteLine($"Start parse from {rng.Start}");
            for (var i = rng.Start.Line; i < rng.End.Line + 1; i++)
            {
                Styles.ClearStyles(i);
                var col = 0;
                state = ParseLine(state.Byte1, ref col, i);
                Lines[i].State = state.Int;
            }
        }

        private State ParseLine(byte section, ref int i, int line)
        {
            var start = i;
            var ln = Lines[line];
            var mys = Sections[section];
            var last = '\0';

            for (; i < ln.Length; i++)
            {
                var c = ln.CharAt(i);

                var kres = IsNonIdent(last) || mys.Keywords.Offset != -1 ?
                    mys.Keywords.Match(c) : -1;

                if (kres > 0 && IsNonIdent(ln.CharAt(i + 1)))
                {
                    Styles.StyleRange(kres, line, i - mys.Keywords.Offset, i);
                    mys.Keywords.Reset();
                }
                else if (kres < 0)
                    mys.Keywords.Reset();

                var sect = mys.Sections.Match(c);

                if (sect != null && !Overlap(mys.Sections, ln, i, sect) && (sect.Start.Length > 1 || IsNonIdent(sect.Start.First()) || IsNonIdent(last)))
                {
                    if (mys.Style != 0)
                        Styles.StyleRange(mys.Style, line, start, ln.Length);

                    i++;
                    var ret = ParseLine(sect.Id, ref i, line);

                    if (i >= ln.Length - 1)
                        return ret;
                }
                else if (mys.End != null && mys.End.Match(c) == MatchResult.Hit)
                {
                    var off1 = mys.StyleCompletely ? mys.Start.Length : 0;
                    Styles.StyleRange(mys.Style, line, start - off1, i - (mys.StyleCompletely ? 0 : mys.End.Offset));
                    return new State { Byte1 = 0 };
                }

                last = c;
            }

            if (mys.Style != 0 && (mys.Multiline || mys.End == null))
            {
                var off = mys.StyleCompletely ? mys.Start.Length : 0;
                Styles.StyleRange(mys.Style, line, start - off, ln.Length);
            }

            if (mys.End != null && mys.Multiline)
            {
                foreach (var ss in Sections)
                    if (ss.Start != null && ss.Start.LastResult == MatchResult.Proc
                        && ss.Start.MatchAnyState)
                        return new State { Byte1 = mys.Id, Byte4 = byte.MaxValue };

                return new State { Byte1 = mys.Id };
            }
            else
                return State.Empty;
        }

        private bool Overlap(IEnumerable<GrammarSection> seq, Line ln, int col, GrammarSection sect)
        {
            for (var la = 1; la < 10; la++)
            {
                var nc = ln.CharAt(col + la);

                if (nc == '\0')
                    return false;

                var match = seq.TryMatch(nc, la - 1, sect);

                if (match != null)
                    return true;
            }

            return false;
        }

        private bool IsNonIdent(char c)
        {
            return c=='\0'||c == ' '||c =='\t'||"`~!@#$%^&*()-=+[{]}\\|;:\",.<>/?".IndexOf(c) != -1;
        }

        internal List<Line> Lines => Context.Buffer.Document.Lines;

        internal StyleManager Styles => Context.Styles;

        public IEditorContext Context { get; private set; }

        public List<GrammarSection> Sections { get; private set; }
    }

    public sealed class GrammarSection
    {
        public byte Id { get; set; }

        public byte ParentId { get; set; }

        public StringTable Keywords { get; } = new StringTable();

        public SectionSequence Start { get; set; }

        public SectionSequence End { get; set; }

        public bool StyleCompletely { get; set; }

        public bool Multiline { get; set; }

        public int Style { get; set; }

        public List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }

    public static class EnumerableExtensions
    {
        public static GrammarSection TryMatch(this IEnumerable<GrammarSection> sections, char c, int shift = 0, GrammarSection except = null)
        {
            foreach (var sect in sections)
                if (sect.Start.TryMatch(c, shift) && sect != except)
                    return sect;

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

    public struct State
    {
        public static readonly State Empty = default(State);
        static byte GetN(int n, int val) => (byte)((val >> (8 * n)) & 0xFF);

        public State(int i4)
        {
            Byte1 = GetN(0, i4);
            Byte2 = GetN(1, i4);
            Byte3 = GetN(2, i4);
            Byte4 = GetN(3, i4);
        }

        public int Int =>
            Byte1 & 0xFF | (Byte2 << 8) | (Byte3 << 16) | (Byte4 << 24);

        public byte Byte1;

        public byte Byte2;

        public byte Byte3;

        public byte Byte4;
    }
}
