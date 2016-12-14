using System;
using CodeBox.Core.ComponentModel;
using System.IO;

namespace CodeBox.Core.ViewModel
{
    public interface IView : IExecutionContext
    {
        void AttachBuffer(IBuffer buffer);

        void DetachBuffer();

        void Close();

        IBuffer Buffer { get; }

        DirectoryInfo Workspace { get; set; }

        string Mode { get; set; }
    }
}
