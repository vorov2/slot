using System;
using Slot.Core;

namespace Slot.Main
{
    public static class Cmd
    {
        public static readonly Identifier CommandPalette = new Identifier("app.commandPalette");
        public static readonly Identifier ChangeMode = new Identifier("app.changeMode");
        public static readonly Identifier ChangeTheme = new Identifier("app.changeTheme");
        public static readonly Identifier ToggleCommandBar = new Identifier("app.toggleCommandBar");
        public static readonly Identifier ToggleMessage = new Identifier("app.toggleMessage");

        public static readonly Identifier NewFile = new Identifier("file.newFile");
        public static readonly Identifier OpenFile = new Identifier("file.openFile");
        public static readonly Identifier OpenRecentFile = new Identifier("file.openRecentFile");
        public static readonly Identifier OpenModifiedFile = new Identifier("file.openModifiedFile");
        public static readonly Identifier ReopenFile = new Identifier("file.reopenFile");
        public static readonly Identifier SaveFile = new Identifier("file.saveFile");
        public static readonly Identifier SaveAll = new Identifier("file.saveAll");
        public static readonly Identifier SaveWithEncoding = new Identifier("file.saveWithEncoding");
        public static readonly Identifier CloseFile = new Identifier("file.closeFile");
        public static readonly Identifier CloseAllFiles = new Identifier("file.closeAllFiles");
        public static readonly Identifier SwitchBuffer = new Identifier("file.switchBuffer");
    }
}
