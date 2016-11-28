using System;

namespace StringMacro
{
    public sealed class SystemInfoVariableProvider : IVariableProvider
    {
        public static readonly SystemInfoVariableProvider Instance = new SystemInfoVariableProvider();
        public static readonly string Prefix = "sys";

        private SystemInfoVariableProvider()
        {

        }

        public bool TryResolve(string key, out string value)
        {
            key = key.ToUpper();
            value = null;
            var retval = true;

            switch (key)
            {
                case "MACHINENAME":
                    value = Environment.MachineName;
                    break;
                case "USERNAME":
                    value = Environment.UserName;
                    break;
                case "USERDOMAINNAME":
                    value = Environment.UserDomainName;
                    break;
                case "PLATFORM":
                    var id = Environment.OSVersion.Platform;
                    value = id == PlatformID.Win32S
                        || id == PlatformID.Win32Windows
                        || id == PlatformID.Win32NT
                        || id == PlatformID.WinCE
                        ? "Win"
                        : id == PlatformID.Unix ? "Unix"
                        : id == PlatformID.MacOSX ? "Mac"
                        : "Unknown";
                    break;
                default:
                    retval = false;
                    break;
            }

            return retval;
        }
    }
}