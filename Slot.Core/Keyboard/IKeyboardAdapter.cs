using System;
using System.Collections.Generic;
using Slot.Core.ComponentModel;

namespace Slot.Core.Keyboard
{
    public interface IKeyboardAdapter : IComponent
    {
        void RegisterInput(Identifier key, string shortcut);

        KeyInput GetCommandShortcut(Identifier key);

        InputState ProcessInput(KeyInput input);

        IEnumerable<KeymapMetadata> EnumerateKeymaps();

        string KeyInputToString(KeyInput input);

        Identifier LastKey { get; }
    }
}
