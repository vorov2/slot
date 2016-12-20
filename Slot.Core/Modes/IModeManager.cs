using System;
using System.Collections.Generic;
using System.IO;
using Slot.Core.ComponentModel;

namespace Slot.Core.Modes
{
    public interface IModeManager : IComponent
    {
        IEnumerable<ModeMetadata> EnumerateModes();

        ModeMetadata SelectMode(FileInfo file);

        ModeMetadata GetMode(Identifier key);
    }
}
