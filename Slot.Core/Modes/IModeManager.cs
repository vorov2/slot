using System;
using System.IO;
using Slot.Core.ComponentModel;

namespace Slot.Core.Modes
{
    public interface IModeManager : IComponent
    {
        ModeMetadata SelectMode(FileInfo file);
    }
}
