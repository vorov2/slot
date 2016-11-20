using CodeBox.Indentation;
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

        internal void Associate(int line, int col, int grammar)
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

        public IDocumentAffinity GetRootAffinity()
        {
            var prov = editor.Styles.Styler as ConfigurableLexer;

            if (prov != null)
                return prov.GrammarProvider.GetGrammar(prov.GrammarKey);

            return null;
        }

        public IDocumentAffinity GetAffinity(Pos pos)
        {
            return GetAffinity(pos.Line, pos.Col);
        }

        public IDocumentAffinity GetAffinity(int line, int col)
        {
            IDocumentAffinity grm = editor.Settings;
            var id = GetAffinityId(line, col);

            if (id != 0)
            {
                var prov = editor.Styles.Styler as ConfigurableLexer;

                if (prov != null)
                    grm = prov.GrammarProvider.GetGrammar(id);
            }

            return grm;
        }

        internal int GetAffinityId(Pos pos)
        {
            return GetAffinityId(pos.Line, pos.Col);
        }

        internal int GetAffinityId(int line, int col)
        {
            var list = default(List<GrammarInfo>);

            if (grammars.TryGetValue(line, out list))
                return list.OrderByDescending(g => g.Col).FirstOrDefault(g => col >= g.Col).GrammarId;

            return 0;
        }
    }
}
