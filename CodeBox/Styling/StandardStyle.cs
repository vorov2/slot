using CodeBox.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public enum StandardStyle
    {
        [FieldName("default")]
        Default = 0,

        [FieldName("selection")]
        Selection = 1,

        [FieldName("symbol.special")]
        SpecialSymbol = 2,

        [FieldName("hyperlink")]
        Hyperlink = 3,

        [FieldName("bracket.matched")]
        MatchedBracket = 4,

        [FieldName("number")]
        Number = 5,

        [FieldName("bracket")]
        Bracket = 6,

        [FieldName("keyword")]
        Keyword = 10,

        [FieldName("keyword.special")]
        KeywordSpecial = 11,

        [FieldName("keyword.typename")]
        TypeName = 12,

        [FieldName("comment")]
        Comment = 13,

        [FieldName("comment.multiline")]
        CommentMultiline = 14,

        [FieldName("comment.document")]
        CommentDocument = 15,

        [FieldName("string")]
        String = 16,

        [FieldName("string.multiline")]
        StringMultiline = 17,

        [FieldName("string.macro")]
        StringMacro = 18,

        [FieldName("char")]
        Char = 19,

        [FieldName("preprocessor")]
        Preprocessor = 20,

        [FieldName("keyword.literal")]
        Literal = 21,

        [FieldName("regex")]
        Regex = 22,

        [FieldName("margin.linenumbers")]
        LineNumbers = 23,

        [FieldName("margin.scrollbar")]
        ScrollBars = 24,

        [FieldName("margin.folding")]
        Folding = 25,

        [FieldName("popup")]
        Popup = 26,

        [FieldName("caret")]
        Caret = 27,
        
        [FieldName("currentline")]
        CurrentLine = 28,

        [FieldName("margin.commands")]
        CommandBar = 29,

        [FieldName("word.matched")]
        MatchedWord = 30,

        [FieldName("search.result")]
        SearchItem = 31,

        [FieldName("error")]
        Error = 32
    }
}
