using CodeBox.Lexing;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Affinity
{
    public sealed class AffinityManager
    {
        private readonly Editor editor;
        private readonly Dictionary<int, List<GrammarInfo>> grammars = new Dictionary<int, List<GrammarInfo>>();

        struct GrammarInfo
        {
            public int GrammarId;
            public int Col;
        }

        internal AffinityManager(Editor editor)
        {
            this.editor = editor;
        }

        internal void AssociateGrammar(int line, int col, int grammar)
        {
            var list = default(List<GrammarInfo>);

            if (!grammars.TryGetValue(line, out list))
                grammars.Add(line, list = new List<GrammarInfo>());

            list.Add(new GrammarInfo { GrammarId = grammar, Col = col });
        }

        internal void ClearAssociations(int line)
        {
            grammars.Remove(line);
        }

        public IDocumentAffinity GetGrammar(Pos pos)
        {
            return GetGrammar(pos.Line, pos.Col);
        }

        public IDocumentAffinity GetGrammar(int line, int col)
        {
            IDocumentAffinity grm = editor.Settings;
            var id = GetGrammarId(line, col);

            if (id != 0)
            {
                var prov = editor.Styles.Provider as ConfigurableLexer;

                if (prov != null)
                    grm = prov.GrammarProvider.GetGrammar(id);
            }

            return grm;
        }

        internal int GetGrammarId(Pos pos)
        {
            return GetGrammarId(pos.Line, pos.Col);
        }

        internal int GetGrammarId(int line, int col)
        {
            var list = default(List<GrammarInfo>);

            if (grammars.TryGetValue(line, out list))
                return list.OrderByDescending(g => g.Col).FirstOrDefault(g => col >= g.Col).GrammarId;

            return 0;
        }

        public string GetNonWordSymbols(Pos pos)
        {
            var grm = GetGrammar(pos);
            return grm.NonWordSymbols ?? editor.Settings.NonWordSymbols;
        }

        public string GetBracketSymbols(Pos pos)
        {
            var grm = GetGrammar(pos);
            return grm.BracketSymbols ?? editor.Settings.BracketSymbols;
        }
    }
}
