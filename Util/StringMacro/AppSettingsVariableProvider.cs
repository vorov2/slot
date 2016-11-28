using System;
using System.Configuration;

namespace StringMacro
{
    public sealed class AppSettingsVariableProvider : IVariableProvider
    {
        public static readonly AppSettingsVariableProvider Instance = new AppSettingsVariableProvider();
        public static readonly string Prefix = "app";

        private AppSettingsVariableProvider()
        {

        }

        public bool TryResolve(string key, out string value)
        {
            value = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(value);
        }
    }
}