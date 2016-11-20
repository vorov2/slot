using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Core.ComponentModel;

namespace CodeBox.ComponentModel
{
    public interface IStylerComponent : IComponent
    {
        void Style(IEditorView context, Range range);
    }
}
