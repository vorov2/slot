using CodeBox.Core;
using CodeBox.Core.CommandModel;
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

        public static void RunCommand(this ComponentCatalog catalog, Editor ctx, Identifier key, params object[] args)
        {
            var disp = catalog.GetComponent(key.Namespace) as ICommandDispatcher;

            if (disp != null)
                disp.Execute(ctx, key, args);
        }
    }
}
