using CodeBox.Indentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class IndentManager
    {
        private readonly Editor editor;

        internal IndentManager(Editor editor)
        {
            this.editor = editor;
        }

        public int CalculateIndentation(int lineIndex) =>
            Provider != null ? Provider.Calculate(lineIndex, editor.Context) : 0;

        public IDentProvider Provider { get; set; }
    }
}
