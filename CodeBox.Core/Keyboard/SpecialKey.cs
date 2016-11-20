using System;

namespace CodeBox.Core.Keyboard
{
    public enum SpecialKey
    {
        None = 0,
        Space = 0x1000,
        Del = 0x1001,
        Tab = 0x1002,
        Home = 0x1003,
        End = 0x1004,
        Ins = 0x1005,
        Back = 0x1006,
        PageUp = 0x1007,
        PageDown = 0x1008,
        Enter = 0x1009,
        Esc = 0x1010,
        F1 = 0x1011,
        F2 = 0x1012,
        F3 = 0x1013,
        F4 = 0x1014,
        F5 = 0x1015,
        F6 = 0x1016,
        F7 = 0x1017,
        F8 = 0x1018,
        F9 = 0x1019,
        F10 = 0x1020,
        F11 = 0x1021,
        F12 = 0x1022,
        Up = 0x1023,
        Down = 0x1024,
        Left = 0x1025,
        Right = 0x1026,

        [FieldName(">")]
        Greater = 0x1027,
        [FieldName("<")]
        Lesser = 0x1028,
        [FieldName("=")]
        Equal = 0x1029,
        [FieldName("-")]
        Minus = 0x1030,
        [FieldName("?")]
        Question = 0x1033,
        [FieldName("\\")]
        Slash = 0x1034,
        [FieldName("[")]
        LeftBracket = 0x1035,
        [FieldName("]")]
        RightBracket = 0x1036,
        [FieldName("'")]
        Quote = 0x1037,
        [FieldName(";")]
        Semicolon = 0x1038,
        [FieldName("~")]
        Tilde = 0x1039,

        Click = 0x2000,
        RightClick = 0x2001,
        DoubleClick = 0x2002,
        Move = 0x2003
    }
}
