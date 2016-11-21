using System;

namespace CodeBox.Core.ComponentModel
{
    public interface ICommandMetadata
    {
        string Key { get; }

        string Alias { get; }

        ArgumentType ArgumentType { get; }

        string ArgumentName { get; }
    }
}
