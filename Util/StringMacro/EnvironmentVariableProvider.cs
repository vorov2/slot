using System;

namespace StringMacro
{
    public sealed class EnvironmentVariableProvider : IVariableProvider
    {
        public static readonly EnvironmentVariableProvider Instance = new EnvironmentVariableProvider();
        public static readonly string Prefix = "env";

        private EnvironmentVariableProvider()
        {

        }

        public bool TryResolve(string key, out string value)
        {
            value = Environment.GetEnvironmentVariable(key);
            return !string.IsNullOrEmpty(value);
        }
    }
}