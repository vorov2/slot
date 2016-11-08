using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public sealed class GrammarSection
    {
        internal bool FoundKeyword { get; set; }

        public byte Id { get; set; }

        public string GrammarKey { get; set; }

        public string ExternalGrammarKey { get; set; }

        public byte ParentId { get; set; }

        public StringTable Keywords { get; } = new StringTable();

        public SectionSequence Start { get; set; }

        public SectionSequence End { get; set; }

        public int StartKeyword { get; set; }

        public bool DontStyleCompletely { get; set; }

        public bool Multiline { get; set; }

        public StandardStyle Style { get; set; }

        public StandardStyle IdentifierStyle { get; set; }

        public StandardStyle FirstIdentifierStyle { get; set; }

        public bool StyleNumbers { get; set; }

        public bool StyleBrackets { get; set; }

        public char ContinuationChar { get; set; }

        public char EscapeChar { get; set; }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }
}
