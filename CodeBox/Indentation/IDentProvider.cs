using System;

namespace CodeBox.Indentation
{
    public interface IDentProvider
    {
        int Calculate(int lineIndex, IEditorContext ctx);
    }
}
