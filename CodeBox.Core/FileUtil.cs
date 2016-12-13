using CodeBox.Core.Output;
using System;
using System.IO;
using System.Text;

namespace CodeBox.Core
{
    public static class FileUtil
    {
        public static bool WriteFile(string fileName, string text, Encoding encoding)
        {
            var fi = default(FileInfo);

            if (!TryGetInfo(fileName, out fi))
                return false;

            return WriteFile(fi, text, encoding);
        }

        public static bool WriteFile(FileInfo fi, string text, Encoding encoding)
        {
            if (!EnsureFilePath(fi))
                return false;

            var res = App.Ext.Handle(() => File.WriteAllText(fi.FullName, text, encoding));

            if (!res.Success)
            {
                App.Ext.Log($"Unable to write file: {res.Reason}", EntryType.Error);
                return false;
            }

            return true;
        }

        public static bool EnsureFilePath(FileInfo fi)
        {
            if (fi == null)
                return false;

            fi.Refresh();

            if (!fi.Directory.Exists)
            {
                var res = App.Ext.Handle(() => fi.Directory.Create());

                if (!res.Success)
                {
                    App.Ext.Log($"Unable to create file: {res.Reason}", EntryType.Error);
                    return false;
                }
            }

            return true;
        }

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
