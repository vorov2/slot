using System;

namespace CodeBox.Commands
{
    internal sealed class CommandInfo
    {
        public int Id { get; set; }

        public IEditorCommand Command { get; set; }
    }
}
