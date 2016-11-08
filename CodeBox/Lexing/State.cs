using System;

namespace CodeBox.Lexing
{
    internal struct State
    {
        public State(int sectionId, string grammarKey)
        {
            SectionId = sectionId;
            GrammarKey = grammarKey;
        }

        public readonly int SectionId;

        public readonly string GrammarKey;
    }
}
