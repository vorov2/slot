using System;
using System.Collections.Generic;
using Slot.Core.ComponentModel;

namespace Slot.Core.Packages
{
    public interface IPackageManager : IComponent
    {
        IEnumerable<PackageMetadata> EnumeratePackages();
    }
}
