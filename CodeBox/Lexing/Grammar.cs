using CodeBox.Affinity;
using CodeBox.Indentation;
using System;
using System.Collections.Generic;

namespace CodeBox.Lexing
{
    public sealed class Grammar : GrammarSection, IDocumentAffinity
    {
        public Grammar()
        {

        }

        #region IDocumentAffinity
        public string NonWordSymbols { get; set; }

        public string BracketSymbols { get; set; }

        public string CommentMask { get; set; }

        public NumberLiteral NumberLiteral { get; set; }

        public string IndentProviderKey { get; set; }
        #endregion

        internal GrammarSection GetSection(int id)
        {
            if (id == 0)
                return this;
            else
                return Sections[id];
        }

        public GrammarSection AddSection(GrammarSection section)
        {
            section.GrammarKey = GrammarKey;
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
            return section;
        }

        internal int GlobalId { get; set; }
    }
}
