using System;
using Slot.Core.ComponentModel;

namespace Slot.Core.Keyboard
{
    public interface IKeyboardAdapter : IComponent
    {
        void RegisterInput(Identifier key, string shortcut);

        KeyInput GetCommandShortcut(Identifier key);

        InputState ProcessInput(KeyInput input);

        void ChangeKeymap(Identifier key);

        Identifier LastKey { get; }
    }
}
