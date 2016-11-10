using CodeBox.ObjectModel;
using System;
using System.Drawing;

namespace CodeBox.Commands
{
    public struct CommandArgument
    {
        public static readonly CommandArgument Empty = new CommandArgument(null);

        internal CommandArgument(char ch)
        {
            Char = ch;
            String = null;
            Pos = Pos.Empty;
            Location = Point.Empty;
        }

        internal CommandArgument(string str)
        {
            Char = '\0';
            String = str;
            Pos = Pos.Empty;
            Location = Point.Empty;
        }

        internal CommandArgument(Pos pos)
        {
            Char = '\0';
            String = null;
            Pos = pos;
            Location = Point.Empty;
        }

        internal CommandArgument(Pos pos, Point loc)
        {
            Char = '\0';
            String = null;
            Pos = pos;
            Location = loc;
        }

        public readonly char Char;
        public readonly string String;
        public readonly Pos Pos;
        public readonly Point Location;
    }
}
