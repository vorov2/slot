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
        const string kw = "true false null switch case default this base get set in new volatile override virtual using namespace readonly static const public private internal protected sealed class struct abstract void int byte long short sbyte uint ushort ulong string char bool object var if else return while for foreach continue break ref out";

        public ConfigurableLexer()
        {
            Sections = new List<GrammarSection>();
            AddSection(new GrammarSection()); //root
            AddSection(new GrammarSection
            {
                Id = 1,
                Start = new SectionSequence("<!", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                StyleCompletely = true,
                Style = (int)StandardStyle.Default
            });
            AddSection(new GrammarSection
            {
                Id = 2,
                Start = new SectionSequence("<", true),
                End = new SectionSequence(">", true),
                Multiline = true,
                StyleCompletely = true,
                IdentifierStyle = 110,
                FirstIdentifierStyle = 113
            }).Keywords.Add("script", (113 & 0xFFFF) | (42 << 16));
            AddSection(new GrammarSection
            {
                Id = 3,
                Start = new SectionSequence("</", true),
                End = new SectionSequence(">", true),
                StyleCompletely = true,
                IdentifierStyle = 113,
                Style = 0
            });
            AddSection(new GrammarSection
            {
                Id = 4,
                StartKeyword = 42,
                End = new SectionSequence("</script>", false),
                Multiline = true,
            }).Keywords.AddRange(kw, (113 & 0xFFFF) | (0 << 16));
            AddSection(new GrammarSection
            {
                Id = 5,
                Start = new SectionSequence("<!--", true),
                End = new SectionSequence("-->", true),
                Multiline = true,
                StyleCompletely = true,
                Style = 112
            });
            AddSection(new GrammarSection
            {
                Id = 6,
                ParentId = 2,
                Start = new SectionSequence("\"", true),
                End = new SectionSequence("\"", true),
                StyleCompletely = true,
                Style = 111
            });


            AddSection(new GrammarSection
            {
                Id = 7,
                ParentId = 4,
                Start = new SectionSequence("//", true),
                StyleCompletely = true,
                Style = 112
            });
            AddSection(new GrammarSection
            {
                Id = 8,
                ParentId = 4,
                Start = new SectionSequence("/*", true),
                End = new SectionSequence("*/", true),
                Multiline = true,
                StyleCompletely = true,
                Style = 112
            });
            AddSection(new GrammarSection
            {
                Id = 9,
                ParentId = 4,
                Start = new SectionSequence("'", true),
                End = new SectionSequence("'", true),
                StyleCompletely = true,
                Style = 111
            });
            AddSection(new GrammarSection
            {
                Id = 10,
                ParentId = 4,
                Start = new SectionSequence("\"", true),
                End = new SectionSequence("\"", true),
                StyleCompletely = true,
                Style = 111
            });
        }

        private GrammarSection AddSection(GrammarSection section)
        {
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
            return section;
        }

        public void Style(IEditorContext context, Range range)
        {
            Context = context;
            Sections.Reset();
            Parse(new State(Lines[range.Start.Line].State), range);
        }

        private void Parse(State state, Range rng)
        {
            Console.WriteLine($"Start parse from {rng.Start}");
            lastKw = 0;
            var lss = 0;
            for (var i = rng.Start.Line; i < rng.End.Line + 1; i++)
            {
                Styles.ClearStyles(i);
                var col = 0;
                state = ParseLine(state.Byte1, ref col, i);

                if (col <= Lines[i].Length - 1)
                    state = ParseLine(state.Byte1, ref col, i);

                Lines[i].State = lss != state.Int ? lss : state.Int;
                lss = state.Int;
            }
        }
        int lastKw = 0;

        private State ParseLine(byte section, ref int i, int line)
        {
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var mys = Sections[section];
            var last = '\0';
            var lastNonIdent = true;

            for (; i < ln.Length; i++)
            {
                var c = ln.CharAt(i);
                var nonIdent = IsNonIdent(c);

                var kres = lastNonIdent || mys.Keywords.Offset != -1 ?
                    mys.Keywords.Match(c) : -1;

                if (kres > 0 && IsNonIdent(ln.CharAt(i + 1)))
                {
                    lastKw = (kres >> 16) & 0xFFFF;
                    Styles.StyleRange(kres & 0xFFFF, line, i - mys.Keywords.Offset, i);
                    identStart = i + 1;
                    mys.Keywords.Reset();
                }
                else if (kres < 0)
                    mys.Keywords.Reset();

                if (nonIdent)
                {
                    if (!lastNonIdent && mys.IdentifierStyle != 0 && i - identStart - 1 > 1)
                        Styles.StyleRange(
                            identStart - start < 3 && mys.FirstIdentifierStyle != 0 ?
                                mys.FirstIdentifierStyle : mys.IdentifierStyle, line, identStart, i - 1);

                    identStart = i + 1;
                }

                var sect = default(GrammarSection);

                //if (lastKw != 0)
                //{
                //    sect = Sections.MatchByKeyword(lastKw);
                //    if (sect != null) { lastKw = 0; }
                //    //i--; }
                //}

                if (sect == null)
                    sect = mys.Sections.Match(c);

                if (sect != null 
                    && (sect.Start == null || 
                    (!Overlap(mys.Sections, ln, i, sect) && (sect.Start.Length > 1 || IsNonIdent(sect.Start.First()) || IsNonIdent(last)))))
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

                    if (!mys.StyleCompletely)
                        i -= mys.End.Offset;

                    if (lastKw != 0 )
                    {
                        sect = Sections[mys.ParentId].Sections.MatchByKeyword(lastKw);
                        if (sect != null)
                        {
                            lastKw = 0;
                            //i--; }
                            return Fetch(sect.Id, sect);
                        }
                    }
                    return Fetch(0, mys);
                }

                last = c;
                lastNonIdent = nonIdent;
            }

            if (mys.Style != 0 && (mys.Multiline || mys.End == null))
            {
                var off = mys.StyleCompletely ? mys.Start.Length : 0;
                Styles.StyleRange(mys.Style, line, start - off, ln.Length);
            }

            if (mys.End != null && mys.Multiline)
            {
                //foreach (var ss in Sections)
                //    if (ss.Start != null && ss.Start.LastResult == MatchResult.Proc
                //        && ss.Start.MatchAnyState)
                //        return new State { Byte1 = mys.Id, Byte4 = byte.MaxValue };

                return Fetch(mys.Id, mys);
            }
            else
                return Fetch(0, mys);
        }

        private State Fetch(byte state, GrammarSection par)
        {
            return state != 0 ? new State { Byte1 = state } :
                new State { Byte1 = par.ParentId };
        }

        private bool Overlap(IEnumerable<GrammarSection> seq, Line ln, int col, GrammarSection sect)
        {
            return seq.TryMatch(ln, col + 1, sect) != null;
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

        public bool StyleCompletely { get; set; }//obsolete

        public bool Multiline { get; set; }

        public int Style { get; set; }

        public int IdentifierStyle { get; set; }

        public int FirstIdentifierStyle { get; set; }

        public int StartKeyword { get; set; }

        public List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }

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
