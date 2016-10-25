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
        }

        internal CommandArgument(string str)
        {
            Char = '\0';
            String = str;
            Pos = default(Pos);
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
