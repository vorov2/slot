using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public sealed class Document
    {
        private int lineSequence;

        private Document()
        {
            Lines = new List<Line>();
            Id = Guid.NewGuid();
        }

        public static Document Read(string source)
        {
            var doc = new Document();
            doc.OriginalEol = source.IndexOf("\r\n") != -1 ? Eol.CrLf
                : source.IndexOf("\n") != -1 ? Eol.Lf
                : Eol.Cr;
            var txt = source.Replace("\r\n", "\n").Replace('\r', '\n');

            foreach (var ln in txt.Split('\n'))
                doc.Lines.Add(doc.NewLine(ln));

            return doc;
        }

        public Line NewLine(string str)
        {
            return Line.FromString(str, ++lineSequence);
        }

        public Line NewLine(IEnumerable<Character> chars)
        {
            return new Line(chars, ++lineSequence);
        }

        public Guid Id { get; private set; }

        public List<Line> Lines { get; private set; }

        internal Eol OriginalEol { get; private set; }
    }
}
