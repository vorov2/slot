using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public struct Character : IEquatable<Character>
    {
        public static readonly Character NewLine = new Character('\n');
        public static readonly Character Empty = new Character('\0');

        private Character(char ch, bool caret)
        {
            Char = ch;
            HasCaret = caret;
        }

        public Character(char ch) : this(ch, false)
        {

        }

        public bool IsEmpty => Char == '\0';

        public bool IsNewLine => Char == '\n';

        public Character WithCaret() => new Character(Char, true);

        public Character WithoutCaret() => new Character(Char, false);

        public readonly bool HasCaret;

        public readonly char Char;

        public static bool operator == (Character fst, Character snd) => Equals(fst, snd);

        public static bool operator !=(Character fst, Character snd) => !Equals(fst, snd);

        public static bool Equals(Character fst, Character snd) => fst.Char == snd.Char;

        public override string ToString() => Char.ToString();

        public bool Equals(Character other) => Char == other.Char;

        public override bool Equals(object obj)
        {
            return obj is Character ? ((Character)obj).Char == Char
                : obj is char ? (char)obj == Char
                : false;
        }

        public override int GetHashCode() => Char.GetHashCode();
    }

    public static class CharacterExtensions
    {
        public static string MakeString(this IEnumerable<Character> chars, Eol eol = Eol.Lf)
        {
            var sb = new StringBuilder(chars.Count());

            foreach (var c in chars)
            {
                if (c.IsNewLine)
                {
                    if (eol == Eol.CrLf)
                        sb.Append("\r\n");
                    else if (eol == Eol.Cr)
                        sb.Append('\r');
                    else
                        sb.Append('\n');
                }
                else
                    sb.Append(c.Char);
            }

            return sb.ToString();
        }

        public static IEnumerable<Character> MakeCharacters(this string str)
        {
            return str.Where(c => c != '\r').Select(c => new Character(c));
        }

        public static IEnumerable<IEnumerable<Character>> MakeLines(this IEnumerable<Character> chars)
        {
            var fst = 0;
            var snd = -1;
            var arr = chars.ToList();
            var ret = new List<IEnumerable<Character>>();

            while ((snd = arr.IndexOf(Character.NewLine, fst)) != -1)
            {
                var ln = arr.GetRange(fst, snd - fst);
                ret.Add(ln);
                fst = snd + 1;
            }

            if (ret.Count == 0)
                ret.Add(arr);
            else
                ret.Add(arr.GetRange(fst, arr.Count - fst));

            return ret;
        }
    }
}
