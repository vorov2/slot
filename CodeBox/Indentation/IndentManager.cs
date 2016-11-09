using CodeBox.Indentation;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Affinity;

namespace CodeBox.Indentation
{
    public sealed class IndentManager
    {
        private readonly Editor editor;

        internal IndentManager(Editor editor)
        {
            this.editor = editor;
        }

        public int CalculateIndentation(int lineIndex)
        {
            var pos = new Pos(lineIndex, 0);
            var prov = editor.AffinityManager.GetAffinity(pos).GetIndentProvider(editor, pos);
            return prov != null ? prov.Calculate(lineIndex, editor) : 0;
        }
    }
}
