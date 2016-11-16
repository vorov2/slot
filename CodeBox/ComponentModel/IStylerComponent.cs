using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ComponentModel
{
    public interface IStylerComponent : IComponent
    {
        void Style(IEditorContext context, Range range);
    }
}
