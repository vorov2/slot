using CodeBox.Core.CommandModel;
using System;
using System.Collections.Generic;

namespace CodeBox.CommandLine
{
    public static class CommandParser
    {
        public static IEnumerable<Statement> Parse(string command)
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

        private static int ParseCommand(Statement stmt, char[] buffer, int pos)
        {
            pos = ParseCommandHead(stmt, buffer, pos);

            if (stmt.Command != null)
                pos = ParseArgument(stmt, buffer, pos);

            return pos;
        }

        private static int ParseCommandHead(Statement stmt, char[] buffer, int pos)
        {
            var start = -1;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);
                var quote = c == '\'' || c == '"';
                var sep = quote || IsSeparator(c);

                if (start > -1 && sep)
                {
                    stmt.Command = new string(buffer, start, pos - start);
                    pos = quote ? pos - 1 : pos;
                    stmt.Location = new Loc(start, pos);
                    return pos;
                }
                else if (start == -1 && !sep)
                    start = pos;
            }

            return buffer.Length;
        }

        private static int ParseArgument(Statement stmt, char[] buffer, int pos)
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
                        stmt.Arguments.Add(new StatementArgument
                        {
                            Location = new Loc(sp, pos - 1),
                            Type = ArgumentType.String,
                            Value = str
                        });

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
                    stmt.Arguments.Add(new StatementArgument
                    {
                        Location = new Loc(start, pos),
                        Type = obj is double ? ArgumentType.Number : ArgumentType.String,
                        Value = obj
                    });
                    start = -1;

                    if (c == ';')
                        return pos + 1;
                }

            }

            return buffer.Length;
        }

        private static object TryConvert(string str)
        {
            double d;

            if (double.TryParse(str, out d))
                return d;
            else
                return str;
        }

        private static int ParseString(Statement stmt, char[] buffer, int pos, char end, out string val)
        {
            var start = pos;
            val = null;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);

                if (c == end || c == '\0')
                {
                    var len = pos - start - (c == '\0' ? 1 : 0);
                    val = new string(buffer, start, len < 0 ? 0 : len);
                    return pos + 1;
                }
            }

            return buffer.Length;
        }

        private static bool IsSeparator(char c)
        {
            return c == ' ' || c == '\t' || c == '\0';
        }

        private static char Lookup(char[] buffer, int pos)
        {
            if (pos > buffer.Length - 1)
                return '\0';
            else
                return buffer[pos];
        }
    }
}
