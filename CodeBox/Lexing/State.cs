using System;

namespace CodeBox.Lexing
{
    internal struct State
    {
        public State(int sectionId, string grammarKey, bool matchAnyState)
        {
            SectionId = sectionId;
            GrammarKey = grammarKey;
            MatchAnyState = matchAnyState;
        }

        public readonly int SectionId;

        public readonly string GrammarKey;

        public readonly bool MatchAnyState;
    }
}
