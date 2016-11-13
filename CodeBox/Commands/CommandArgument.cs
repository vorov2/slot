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
        }

        internal CommandArgument(string str)
        {
            Char = '\0';
            String = str;
            Pos = Pos.Empty;
        }

        internal CommandArgument(Pos pos)
        {
            Char = '\0';
            String = null;
            Pos = pos;
        }

        public readonly char Char;
        public readonly string String;
        public readonly Pos Pos;
    }
}
