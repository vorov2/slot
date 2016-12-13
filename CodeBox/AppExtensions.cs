using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Lexing;
using System;

namespace CodeBox
{
    public static class AppExtensions
    {
        private static IGrammarComponent _grammar;

        public static IGrammarComponent Grammars(this IAppExtensions _)
        {
            if (_grammar == null)
                _grammar = App.Catalog<IGrammarComponent>().Default();
            return _grammar;
        }
    }
}
