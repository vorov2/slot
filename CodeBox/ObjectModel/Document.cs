using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class Document
    {
        internal Document()
        {
            Lines = new List<Line>();
            Id = Guid.NewGuid();
            Selections = new Selections();
        }

        internal void ReindexLines()
        {
            for (var i = 0; i < Lines.Count; i++)
                Lines[i].Index = i;
        }
        
        internal int LineIndexById(int id)
        {
            for (var i = 0; i < Lines.Count; i++)
            {
                var ln = Lines[i];

                if (ln.Id == id)
                    return i;
            }

            return -1;
        }

        public Guid Id { get; private set; }

        internal int LineSequence { get; set; }

        internal List<Line> Lines { get; private set; }

        internal Selections Selections { get; private set; }
    }
}
