using System;
using Slot.Core;

namespace Slot.Editor.Lexing
{
    internal struct State
    {
        public State(int sectionId, Identifier grammarKey, bool matchAnyState)
        {
            SectionId = sectionId;
            GrammarKey = grammarKey;
            MatchAnyState = matchAnyState;
        }

        public readonly int SectionId;

        public readonly Identifier GrammarKey;

        public readonly bool MatchAnyState;
    }
}
