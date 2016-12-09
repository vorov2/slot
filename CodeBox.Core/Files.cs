using System;
using System.IO;

namespace CodeBox.Core
{
    public static class Files
    {
        public static bool TryGetInfo(string name, out FileInfo fileInfo)
        {
            try
            {
                fileInfo = new FileInfo(name);
                return true;
            }
            catch (Exception)
            {
                fileInfo = null;
                return false;
            }
        }
    }
}
