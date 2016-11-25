using CodeBox.Core;
using CodeBox.Core.ComponentModel;
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
    internal struct GrammarInfo
    {
        public int GrammarId;
        public int Col;
    }

    public sealed class AffinityManager
    {
        private readonly Editor editor;

        internal AffinityManager(Editor editor)
        {
            this.editor = editor;
        }

        internal void Associate(int line, int col, int grammar)
        {
            var ln = editor.Lines[line];
            if (ln.Grammars.Count == 0 || ln.Grammars[ln.Grammars.Count - 1].GrammarId != grammar)
                ln.Grammars.Add(new GrammarInfo { GrammarId = grammar, Col = col });
        }

        internal void ClearAssociations(int line)
        {
            editor.Lines[line].Grammars.Clear();
        }

        public IDocumentAffinity GetRootAffinity()
        {
            return ComponentCatalog.Instance.Grammars().GetGrammar(editor.Buffer.GrammarKey);
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
                grm = ComponentCatalog.Instance.Grammars().GetGrammar(id);

            return grm;
        }

        internal int GetAffinityId(Pos pos)
        {
            return GetAffinityId(pos.Line, pos.Col);
        }

        internal int GetAffinityId(int line, int col)
        {
            var ln = editor.Lines[line];
            return ln.Grammars.OrderByDescending(g => g.Col).FirstOrDefault(g => col >= g.Col).GrammarId;
        }
    }
}
