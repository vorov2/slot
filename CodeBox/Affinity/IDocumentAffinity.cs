using CodeBox.Indentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Affinity
{
    public interface IDocumentAffinity
    {
        string NonWordSymbols { get; }

        string BracketSymbols { get; }

        string CommentMask { get; }

        IDentProvider IndentProvider { get; set; }
    }
}
