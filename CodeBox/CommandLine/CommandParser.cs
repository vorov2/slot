using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CodeBox.CommandLine
{
    public static class CommandParser
    {
        public static Statement Parse(string command)
        {
            var buffer = command.ToCharArray();
            var pos = 0;

            var stmt = new Statement();
            pos = ParseCommand(stmt, buffer, pos);

            if (stmt.Command != null)
                return stmt;

            return null;
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

        private static int ParseArgument(Statement stmt, char[] buffer, int pos)
        {
            var start = -1;

            for (; pos < buffer.Length + 1; pos++)
            {
                var c = Lookup(buffer, pos);
                var sep = IsSeparator(c);

                if (!sep && start == -1)
                    start = pos;
                else if (c == '\0' && start > -1)
                {
                    var str = new string(buffer, start, pos - start);
                    stmt.Argument = TryConvert(str);
                    stmt.ArgumentType = stmt.Argument is double ? ArgumentType.Number : ArgumentType.String;
                    stmt.ArgumentLocation = new Loc(start, pos);
                    return pos;
                }
            }

            return buffer.Length;
        }

        private static object TryConvert(string str)
        {
            double d;

            if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out d))
                return d;
            else
                return str;
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
