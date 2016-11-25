using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using CodeBox.Lexing;
using System;

namespace CodeBox
{
    public static class ComponentCatalogExtensions
    {
        private static IGrammarComponent _grammar;

        public static IGrammarComponent Grammars(this ComponentCatalog catalog)
        {
            if (_grammar == null)
                _grammar = ((IGrammarComponent)catalog
                        .GetComponent((Identifier)"grammar.default"));
            return _grammar;
        }
    }
}
