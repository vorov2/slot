using Slot.Core;
using Slot.Core.ComponentModel;
using Slot.Editor.Indentation;
using Slot.Editor.Lexing;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.Affinity
{
    internal struct GrammarInfo
    {
        public int GrammarId;
        public int Col;
    }

    public sealed class AffinityManager
    {
        private readonly EditorControl editor;

        internal AffinityManager(EditorControl editor)
        {
            this.editor = editor;
        }

        internal void Associate(int line, int col, int grammar)
        {
            var ln = editor.Lines[line];
            if (ln.Grammars.Count == 0 || ln.Grammars[ln.Grammars.Count - 1].GrammarId != grammar)
                ln.Grammars.Add(new GrammarInfo { GrammarId = grammar, Col = col });
        }

        internal void ClearAssociations(int line) => editor.Lines[line].Grammars.Clear();

        public IDocumentAffinity GetRootAffinity() => App.Ext.Grammars().GetGrammar(editor.Buffer.GrammarKey);

        public IDocumentAffinity GetAffinity(Pos pos) => GetAffinity(pos.Line, pos.Col);

        public IDocumentAffinity GetAffinity(int line, int col)
        {
            IDocumentAffinity grm = null;
            var id = GetAffinityId(line, col);

            if (id != 0)
                grm = App.Ext.Grammars().GetGrammar(id);

            return grm ?? GetRootAffinity() ?? editor.EditorSettings;
        }

        internal int GetAffinityId(Pos pos) => GetAffinityId(pos.Line, pos.Col);

        internal int GetAffinityId(int line, int col) =>
            editor.Lines[line].Grammars.OrderByDescending(g => g.Col).FirstOrDefault(g => col >= g.Col).GrammarId;
    }
}
