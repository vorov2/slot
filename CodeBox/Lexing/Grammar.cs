using CodeBox.Affinity;
using CodeBox.Core;
using CodeBox.Indentation;
using System;
using System.Collections.Generic;

namespace CodeBox.Lexing
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

        public string Key { get; set; }

        public List<string> Extensions { get; } = new List<string>();

        public GrammarSection AddSection(GrammarSection section)
        {
            section.GrammarKey = Key;
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
            return section;
        }

        internal int GlobalId { get; set; }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }
}
