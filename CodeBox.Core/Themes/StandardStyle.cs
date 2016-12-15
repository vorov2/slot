using System;

namespace CodeBox.Core.Themes
{
    public enum StandardStyle
    {
        [FieldName("default")]
        Default = 0,

        [FieldName("selection")]
        Selection,

        [FieldName("symbol.special")]
        SpecialSymbol,

        [FieldName("hyperlink")]
        Hyperlink,

        [FieldName("bracket.matched")]
        MatchedBracket,

        [FieldName("hint")]
        Hint,

        [FieldName("number")]
        Number,

        [FieldName("bracket")]
        Bracket,

        [FieldName("keyword")]
        Keyword,

        [FieldName("keyword.special")]
        KeywordSpecial,

        [FieldName("keyword.typename")]
        TypeName,

        [FieldName("comment")]
        Comment,

        [FieldName("comment.multiline")]
        CommentMultiline,

        [FieldName("comment.document")]
        CommentDocument,

        [FieldName("string")]
        String,

        [FieldName("string.multiline")]
        StringMultiline,

        [FieldName("string.macro")]
        StringMacro,

        [FieldName("char")]
        Char,

        [FieldName("preprocessor")]
        Preprocessor,

        [FieldName("keyword.literal")]
        Literal,

        [FieldName("regex")]
        Regex,

        [FieldName("margin.linenumbers")]
        LineNumbers,

        [FieldName("margin.currentlinenumber")]
        CurrentLineNumber,

        [FieldName("margin.scrollbar")]
        ScrollBars,

        [FieldName("margin.activescrollbar")]
        ActiveScrollBar,

        [FieldName("margin.folding")]
        Folding,

        [FieldName("margin.activefolding")]
        ActiveFolding,

        [FieldName("popup")]
        Popup,

        [FieldName("popuphover")]
        PopupHover,

        [FieldName("popupselected")]
        PopupSelected,

        [FieldName("popupborder")]
        PopupBorder,

        [FieldName("caret")]
        Caret,
        
        [FieldName("currentline")]
        CurrentLine,

        [FieldName("margin.commands")]
        CommandBar,

        [FieldName("margin.commandscaption")]
        CommandBarCaption,

        [FieldName("word.matched")]
        MatchedWord,

        [FieldName("search.result")]
        SearchItem,

        [FieldName("error")]
        Error,

        [FieldName("margin.status")]
        StatusBar,

        [FieldName("margin.activestatus")]
        ActiveStatusBar
    }
}
