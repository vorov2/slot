using Slot.Core;

namespace Slot.Editor.Affinity
{
    public static class DocumentAffinityExtensions
    {
        public static string GetNonWordSymbols(this IDocumentAffinity aff, EditorControl ctx)
        {
            return aff.NonWordSymbols ?? ctx.Settings.NonWordSymbols;
        }

        public static string GetBracketSymbols(this IDocumentAffinity aff, EditorControl ctx)
        {
            return aff.BracketSymbols ?? ctx.Settings.BracketSymbols;
        }

        public static string GetAutocompleteSymbols(this IDocumentAffinity aff, EditorControl ctx)
        {
            return aff.AutocompleteSymbols ?? ctx.Settings.AutocompleteSymbols;
        }

        public static Identifier GetIndentComponentKey(this IDocumentAffinity aff, EditorControl ctx)
        {
            return aff.IndentComponentKey ?? ctx.Settings.IndentComponentKey;
        }
    }
}
