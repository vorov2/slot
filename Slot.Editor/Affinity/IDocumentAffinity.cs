using Slot.Core;
using Slot.Editor.Indentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.Affinity
{
    public interface IDocumentAffinity
    {
        string NonWordSymbols { get; }

        string BracketSymbols { get; }

        string CommentMask { get; }

        string AutocompleteSymbols { get; }

        NumberLiteral NumberLiteral { get; }

        Identifier IndentComponentKey { get; }

        Identifier FoldingComponentKey { get; }
    }
}
