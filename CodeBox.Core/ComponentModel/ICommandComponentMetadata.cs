using System;

namespace CodeBox.Core.ComponentModel
{
    public interface ICommandComponentMetadata
    {
        string Key { get; }

        string Alias { get; }
    }
}
