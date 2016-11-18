using CodeBox.Indentation;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Affinity
{
    public static class DocumentAffinityExtensions
    {
        public static string GetNonWordSymbols(this IDocumentAffinity aff, IEditorContext ctx)
        {
            return aff.NonWordSymbols ?? ctx.Settings.NonWordSymbols;
        }

        public static string GetBracketSymbols(this IDocumentAffinity aff, IEditorContext ctx)
        {
            return aff.BracketSymbols ?? ctx.Settings.BracketSymbols;
        }

        public static string GetAutocompleteSymbols(this IDocumentAffinity aff, IEditorContext ctx)
        {
            return aff.AutocompleteSymbols ?? ctx.Settings.AutocompleteSymbols;
        }

        public static string GetIndentComponentKey(this IDocumentAffinity aff, IEditorContext ctx)
        {
            return aff.IndentComponentKey ?? ctx.Settings.IndentComponentKey;
        }
    }
}
