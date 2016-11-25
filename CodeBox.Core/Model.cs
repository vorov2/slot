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


        FileInfo File { get; }
    }

    public interface IView
    {
        IBuffer Buffer { get; }

        bool IsDirty { get; }
    }

    public interface IViewManager
    {

    }
}
