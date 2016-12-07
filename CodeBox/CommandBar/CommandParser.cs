using CodeBox.Core.CommandModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CodeBox.CommandBar
{
    public sealed class CommandParser
    {
        private readonly Statement statement;

        public CommandParser()
        {

        }

        public CommandParser(Statement statement)
        {
            this.statement = statement;
        }

        public Statement Parse(string command)
        {
            var buffer = command.ToCharArray();
            var pos = 0;
            var stmt = statement ?? new Statement();
            stmt.Location = default(Loc);
            ParseCommand(stmt, buffer, pos);
            return stmt;
        }

        private int ParseCommand(Statement stmt, char[] buffer, int pos)
        {
            pos = stmt.Command == null ? ParseCommandHead(stmt, buffer, pos) : 0;

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

        private int ParseArgument(Statement stmt, char[] buffer, int pos)
        {
            var start = -1;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);
                var ws = c == ' ' || c == '\t';
                var sep = c == '|' || c == '\0';

                if (start == -1 && (c == '"' || c == '\''))
                {
                    string str;
                    var sp = pos;
                    pos = ParseString(stmt, buffer, pos + 1, c, out str);

                    if (str != null)
                        stmt.Arguments.Add(new StatementArgument
                        {
                            Location = new Loc(sp, pos),
                            Value = str
                        });

                    start = -1;
                }
                else if (!sep && !ws && start == -1)
                    start = pos;
                else if (sep && start > -1)
                {
                    var str = new string(buffer, start, pos - start);
                    stmt.Arguments.Add(new StatementArgument
                    {
                        Location = new Loc(start, c == '|' ? pos - 1 : pos),
                        Value = str
                    });
                    start = -1;
                }

            }

            return buffer.Length;
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
                    if (c == end && Lookup(buffer, pos + 1) == end)
                    {
                        pos++;
                        continue;
                    }

                    var len = pos - start;
                    val = new string(buffer, start, len < 0 ? 0 : len);
                    return pos;
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
}
