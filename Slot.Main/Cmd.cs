using System;
using Slot.Core;

namespace Slot.Main
{
    public static class Cmd
    {
        public static readonly Identifier CommandPalette = new Identifier("app.commandPalette");
        public static readonly Identifier ChangeMode = new Identifier("app.changeMode");

        public static readonly Identifier OpenFile = new Identifier("file.openFile");
        public static readonly Identifier ReopenFile = new Identifier("file.reopenFile");
        public static readonly Identifier SaveFile = new Identifier("file.save");
    }
}
