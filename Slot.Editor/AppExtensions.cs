using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Editor.Lexing;
using System;

namespace Slot.Editor
{
    public static class AppExtensions
    {
        private static IGrammarComponent _grammar;

        public static IGrammarComponent Grammars(this IAppExtensions _)
        {
            if (_grammar == null)
                _grammar = App.Component<IGrammarComponent>();
            return _grammar;
        }
    }
}
