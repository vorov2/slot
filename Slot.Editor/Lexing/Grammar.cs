using Slot.Editor.Affinity;
using Slot.Core;
using System;
using System.Collections.Generic;

namespace Slot.Editor.Lexing
{
    public sealed class Grammar : IDocumentAffinity
    {
        public Grammar()
        {

        }

        #region IDocumentAffinity
        public string NonWordSymbols { get; set; }

        public string BracketSymbols { get; set; }

        public string CommentMask { get; set; }

        public NumberLiteral NumberLiteral { get; set; }

        public Identifier IndentComponentKey { get; set; }

        public Identifier FoldingComponentKey { get; set; }

        public string AutocompleteSymbols { get; set; }
        #endregion

        public Identifier StylerKey { get; set; }

        internal int GlobalId { get; set; }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }
}
