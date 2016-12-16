using Slot.Core.Themes;
using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.Lexing
{
    public class GrammarSection
    {
        public GrammarSection()
        {

        }

        internal GrammarSection BackDelegate;
        internal bool Fallback;

        public int Id { get; set; }

        public string GrammarKey { get; set; }

        public string ExternalGrammarKey { get; set; }

        public int ParentId { get; set; }

        public bool IgnoreCase { get; set; }

        public StringTable Keywords { get; set; }

        public SectionSequence Start { get; set; }

        public SectionSequence End { get; set; }

        public bool DontStyleCompletely { get; set; }

        public bool Multiline { get; set; }

        public StandardStyle Style { get; set; }

        public StandardStyle IdentifierStyle { get; set; }

        public StandardStyle ContextIdentifierStyle { get; set; }

        public bool StyleNumbers { get; set; }

        public bool StyleBrackets { get; set; }

        public string ContextChars { get; set; }

        public char ContinuationChar { get; set; }

        public char EscapeChar { get; set; }

        public char TerminatorChar { get; set; }

        public char TerminatorEndChar { get; set; }

        public bool OnLineStartOnly { get; set; }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }
}
