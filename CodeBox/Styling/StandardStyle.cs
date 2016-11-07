using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public enum StandardStyle
    {
        Default = 0,
        Selection = 1,
        SpecialSymbol = 2,
        Hyperlink = 3,
        MatchedBracket = 4,
        Number = 5,
        Bracket = 6,

        Keyword = 10,
        KeywordSpecial = 11,
        KeywordType = 12,
        Comment = 13,
        CommentMultiline = 14,
        CommentDocument = 15,
        String = 16,
        StringMultiline = 17,
        StringSplice = 18,
        Char = 19,
        Preprocessor = 20,
        Literal = 21,
        Regex = 22,
        KeywordModifier = 23
    }
}
