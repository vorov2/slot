using System;

namespace CodeBox.ObjectModel
{
    public struct CommandArgument
    {
        internal CommandArgument(char ch, string str)
        {
            Char = ch;
            String = str;
        }

        public readonly char Char;
        public readonly string String;
    }
}
