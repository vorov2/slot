using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Core;
using Slot.Core.Settings;

namespace Slot.Editor.ObjectModel
{
    public sealed class Document
    {
        private Document()
        {
            Lines = new List<Line>();
            Id = Guid.NewGuid();
        }

        public static Document FromString(string source)
        {
            var doc = new Document();
            doc.OriginalEol = source.IndexOf("\r\n") != -1 ? Eol.CrLf
                : source.IndexOf("\n") != -1 ? Eol.Lf
                : source.IndexOf("\r") != -1 ? Eol.Cr
                : (App.IsMono ? Eol.Lf : Eol.CrLf);
            var txt = source.Replace("\r\n", "\n").Replace('\r', '\n');

            foreach (var ln in txt.Split('\n'))
                doc.Lines.Add(Line.FromString(ln));

            return doc;
        }

        public Line GetLine(int index) => Lines[index];

        public Guid Id { get; private set; }

        internal List<Line> Lines { get; private set; }

        internal Eol OriginalEol { get; private set; }
    }
}
