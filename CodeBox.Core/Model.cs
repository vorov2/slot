using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core
{
    public interface IBuffer
    {
        bool IsDirty { get; }

        FileInfo File { get; }
    }

    public interface IView
    {
        IBuffer Buffer { get; }
    }

    public interface IViewManager
    {
        IView CreateView(IBuffer buffer);

        IView GetActiveView();
    }
}
