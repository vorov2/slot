using CodeBox.Indentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Affinity
{
    public interface IDocumentAffinity
    {
        string NonWordSymbols { get; }

        string BracketSymbols { get; }

        string CommentMask { get; }

        string AutocompleteSymbols { get; }

        NumberLiteral NumberLiteral { get; }

        string IndentComponentKey { get; set; }
    }
}
