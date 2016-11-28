using System;
using System.Collections.Generic;

namespace StringMacro
{
    public sealed class VariableProviders
    {
        private readonly Dictionary<string, IVariableProvider> providers;
        internal readonly static VariableProviders Empty = new VariableProviders(); 
        
        public VariableProviders()
        {
            providers = new Dictionary<string, IVariableProvider>();
        }

        public bool Register(string prefix, IVariableProvider provider)
        {
            if (!providers.ContainsKey(prefix))
            {
                providers.Add(prefix, provider);
                return true;
            }
            else
                return false;
        }

        internal IVariableProvider GetProvider(string prefix)
        {
            IVariableProvider ret;
            providers.TryGetValue(prefix, out ret);
            return ret;
        }
        
        private static VariableProviders _default;
        public static VariableProviders Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new VariableProviders();
                    _default.Register(AppSettingsVariableProvider.Prefix,
                        AppSettingsVariableProvider.Instance);
                    _default.Register(FolderVariableProvider.Prefix,
                        FolderVariableProvider.Instance);
                    _default.Register(EnvironmentVariableProvider.Prefix,
                        EnvironmentVariableProvider.Instance);
                    _default.Register(SystemInfoVariableProvider.Prefix,
                        SystemInfoVariableProvider.Instance);
                }

                return _default;
            }
        }

    }
}