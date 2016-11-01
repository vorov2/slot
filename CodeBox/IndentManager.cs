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

        public int CalculateIndentation(int lineIndex)
        {
            var p = Provider;

            if (p != null)
                return p.Calculate(lineIndex, editor.Context);
            else
                return 0;
        }

        public IDentProvider Provider { get; set; }
    }
}
