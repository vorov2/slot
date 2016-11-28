using System;

namespace StringMacro
{
    public interface IVariableProvider
    {
        bool TryResolve(string key, out string value);
    }
}