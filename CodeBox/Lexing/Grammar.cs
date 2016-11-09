using CodeBox.Affinity;
using CodeBox.Indentation;
using System;
using System.Collections.Generic;

namespace CodeBox.Lexing
{
    public sealed class Grammar : IDocumentAffinity
    {
        public Grammar(string key)
        {
            Key = key;
        }

        internal int Id { get; set; }

        public string Key { get; }

        #region IDocumentAffinity
        public string NonWordSymbols { get; set; }

        public string BracketSymbols { get; set; }

        public IDentProvider IndentProvider { get; set; }
        #endregion

        public GrammarSection AddSection(GrammarSection section)
        {
            section.GrammarKey = Key;
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
            return section;
        }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }

}
