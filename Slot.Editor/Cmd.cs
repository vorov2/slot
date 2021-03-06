﻿using System;
using Slot.Core;
using Slot.Editor.Commands;

namespace Slot.Editor
{
    public static class Cmd
    {
        public static readonly Identifier SetBufferEol = new Identifier("buffer.setBufferEol");
        public static readonly Identifier ToggleWordWrap = new Identifier("buffer.toggleWordWrap");
        public static readonly Identifier GotoLine = new Identifier(GotoLineCommand.Name);
        public static readonly Identifier InsertChar = new Identifier(InsertCharCommand.Name);
        public static readonly Identifier InsertRange = new Identifier(InsertRangeCommand.Name);
    }
}
