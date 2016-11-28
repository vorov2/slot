using System;

namespace CodeBox.Core.ViewModel
{
    public interface IView
    {
        void AttachBuffer(IBuffer buffer);

        void DetachBuffer();

        IBuffer Buffer { get; }
    }
}
