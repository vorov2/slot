using CodeBox.ObjectModel;
using System;
using System.Drawing;

namespace CodeBox.Commands
{
    public struct CommandArgument
    {
        internal CommandArgument(char ch)
        {
            Char = ch;
            String = null;
            Pos = default(Pos);
            Location = default(Point);
        }

        internal CommandArgument(string str)
        {
            Char = '\0';
            String = str;
            Pos = default(Pos);
            Location = default(Point);
        }

        internal CommandArgument(Pos pos)
        {
            Char = '\0';
            String = null;
            Pos = pos;
            Location = default(Point);
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
