using System;

namespace CodeBox.Lexing
{
    internal sealed class ParseState
    {
        public char Context;

        public GrammarSection BackDelegate;
    }
}
