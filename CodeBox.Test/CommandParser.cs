using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Test
{
    public sealed class CommandParser
    {
        public IEnumerable<Statement> Parse(string command)
        {
            var buffer = command.ToCharArray();
            var pos = 0;

            do
            {
                var stmt = new Statement();
                pos = ParseCommand(stmt, buffer, pos);

                if (stmt.Command != null)
                    yield return stmt;

            }
            while (pos < buffer.Length);
        }

        private int ParseCommand(Statement stmt, char[] buffer, int pos)
        {
            pos = ParseCommandHead(stmt, buffer, pos);

            if (stmt.Command != null)
                pos = ParseArgument(stmt, buffer, pos);

            return pos;
        }

        private int ParseCommandHead(Statement stmt, char[] buffer, int pos)
        {
            var start = -1;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);
                var sep = IsSeparator(c);

                if (start > -1 && sep)
                {
                    stmt.Command = new string(buffer, start, pos - start);
                    stmt.Location = new Loc(start, pos);
                    return pos;
                }
                else if (start == -1 && !sep)
                    start = pos;
            }

            return buffer.Length;
        }

        private int ParseArgument(Statement stmt, char[] buffer, int pos)
        {
            var start = -1;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);
                var sep = IsSeparator(c);

                if (start == -1 && (c == '"' || c == '\''))
                {
                    string str;
                    var sp = pos;
                    pos = ParseString(stmt, buffer, pos + 1, c, out str);

                    if (str != null)
                        stmt.Arguments.Add(new StatementArgument {
                            Location = new Loc(sp, pos - 1), Value = str });

                    start = -1;
                }
                else if (start == -1 && c == ';')
                    return pos + 1;
                else if (!sep && start == -1)
                    start = pos;
                else if ((sep || c == ';') && start > -1)
                {
                    var str = new string(buffer, start, pos - start);
                    var obj = TryConvert(str);
                    stmt.Arguments.Add(new StatementArgument {
                        Location = new Loc(start, pos), Value = obj });
                    start = -1;

                    if (c == ';')
                        return pos + 1;
                }

            }

            return buffer.Length;
        }

        private object TryConvert(string str)
        {
            double d;

            if (double.TryParse(str, out d))
                return d;
            else
                return str;
        }

        private int ParseString(Statement stmt, char[] buffer, int pos, char end, out string val)
        {
            var start = pos;
            val = null;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);

                if (c == end || c == '\0')
                {
                    val = new string(buffer, start, pos - start - (c == '\0' ? 1 : 0));
                    return pos + 1;
                }
            }

            return buffer.Length;
        }

        private bool IsSeparator(char c)
        {
            return c == ' ' || c == '\t' || c == '\0';
        }

        private char Lookup(char[] buffer, int pos)
        {
            if (pos > buffer.Length - 1)
                return '\0';
            else
                return buffer[pos];
        }
    }

    public sealed class Statement
    {
        internal Statement()
        {

        }

        public Loc Location { get; internal set; }

        public string Command { get; internal set; }

        public List<StatementArgument> Arguments { get; } = new List<StatementArgument>();
    }

    public sealed class StatementArgument
    {
        internal StatementArgument()
        {

        }

        public Loc Location { get; internal set; }

        public object Value { get; internal set; }
    }

    public struct Loc
    {
        public Loc(int start, int end)
        {
            Start = start;
            End = end;
        }

        public readonly int Start;
        public readonly int End;
    }
}
