using System;
using CodeBox.Core;

namespace CodeBox
{
    public static class Cmd
    {
        public static readonly Identifier SetBufferEol = new Identifier("buffer.setBufferEol");
        public static readonly Identifier ToggleWordWrap = new Identifier("buffer.toggleWordWrap");
        public static readonly Identifier ChangeBufferMode = new Identifier("buffer.changeBufferMode");
    }
}
