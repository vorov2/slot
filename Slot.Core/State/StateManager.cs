using System;
using System.ComponentModel.Composition;
using System.IO;
using Slot.Core.ComponentModel;

namespace Slot.Core.State
{
    [Export(typeof(IStateManager))]
    [ComponentData(Name)]
    public sealed class StateManager : IStateManager
    {
        public const string Name = "statemanager.default";

        [Import("directory.user.state")]
        private string statePath = null;

        public Stream ReadState(Guid stateId)
        {
            var fn = GetFileInfo(stateId);

            if (fn.Exists)
                return fn.OpenRead();
            else
                return null;
        }

        public Stream WriteState(Guid stateId)
        {
            var fn = GetFileInfo(stateId);

            if (!FileUtil.EnsureFilePath(fn))
                return null;

            return File.Open(fn.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }

        private FileInfo GetFileInfo(Guid stateId) =>
            new FileInfo(Path.Combine(statePath, stateId.ToString()));
    }
}
