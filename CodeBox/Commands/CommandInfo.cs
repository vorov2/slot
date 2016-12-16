using System;

namespace Slot.Editor.Commands
{
    internal sealed class CommandInfo
    {
        public int Id { get; set; }

        public EditorCommand Command { get; set; }
    }
}
