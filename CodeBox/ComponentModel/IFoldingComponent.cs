using CodeBox.Commands;
using CodeBox.Core.ComponentModel;
using CodeBox.ObjectModel;
using System;

namespace CodeBox.ComponentModel
{
    public interface IFoldingComponent : IComponent
    {
        void Fold(IEditorView ctx, Range range);
    }
}
