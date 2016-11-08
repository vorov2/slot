using CodeBox.Lexing;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Grammars
{
    public sealed class GrammarManager
    {
        private readonly Editor editor;

        internal GrammarManager(Editor editor)
        {
            this.editor = editor;
        }

        public IGrammar GetGrammar(Line line)
        {
            IGrammar grm = editor.Settings;

            if (line.GrammarId != 0)
            {
                var prov = editor.Styles.Provider as ConfigurableLexer;

                if (prov != null)
                    grm = prov.GrammarProvider.GetGrammar(line.GrammarId);
            }

            return grm;
        }
        
        public string GetNonWordSymbols(Line line)
        {
            var grm = GetGrammar(line);
            return grm.NonWordSymbols ?? editor.Settings.NonWordSymbols;
        }
    }
}
