using Slot.Core.Output;
using Slot.Core.ViewModel;
using System;
using System.IO;
using System.Text;

namespace Slot.Core
{
    public static class FileUtil
    {
        public static bool HasBom(string fileName)
        {
            FileInfo fi;

            if (!TryGetInfo(fileName, out fi))
                return false;

            return HasBom(fi);
        }

        public static bool HasBom(FileInfo fileName)
        {
            var bom = false;

            App.Ext.Handle(() =>
            {
                using (var fs = new FileStream(fileName.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bits = new byte[3];
                    fs.Read(bits, 0, 3);
                    bom = bits[0] == 0xEF && bits[1] == 0xBB && bits[2] == 0xBF;
                }
            });

            return bom;
        }

        public static string GenerateFileName(string mask)
        {
            var tag = DateTime.Now.ToString("ddMMyyyyHHmm");
            var name = string.Format(mask, tag);
            var attempt = 0;

            if (File.Exists(name))
            {
                attempt++;
                name = string.Format(mask, tag + "(" + attempt + ")");
            }

            return name;
        }

        public static bool ReadFile(string fileName, Encoding encoding, out string content)
        {
            FileInfo fi;
            content = null;

            if (!TryGetInfo(fileName, out fi))
                return false;

            return ReadFile(fi, encoding, out content);
        }

        public static bool ReadFile(FileInfo fileName, Encoding encoding, out string content)
        {
            var data = "";
            var res = App.Ext.Handle(() =>
            {
                using (var fs = new StreamReader(fileName.FullName, encoding, false))
                    data = fs.ReadToEnd();
            });

            if (!res.Success)
                App.Ext.Log($"Unable to read file: {res.Reason}", EntryType.Error);

            content = data;

            if (res.Success)
                App.Ext.Log($"File read: {fileName}", EntryType.Info);

            return res.Success;
        }

        public static bool WriteFile(string fileName, string text, Encoding encoding)
        {
            var fi = default(FileInfo);

            if (!TryGetInfo(fileName, out fi))
                return false;

            var ret = WriteFile(fi, text, encoding);

            if (ret)
                App.Ext.Log($"File written: {fileName}", EntryType.Info);

            return ret;
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
            return EnsurePath(fi.Directory);
        }

        public static bool EnsurePath(DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                var res = App.Ext.Handle(() => dir.Create());

                if (!res.Success)
                {
                    App.Ext.Log($"Unable to create file: {res.Reason}", EntryType.Error);
                    return false;
                }
            }

            return true;
        }

        public static bool TryDelete(FileInfo fi)
        {
            if (fi == null)
                return false;

            fi.Refresh();

            if (!fi.Exists)
                return true;

            var res = App.Ext.Handle(() => File.Delete(fi.FullName));

            if (!res.Success)
                App.Ext.Log($"Unable to delete file: {res.Reason}", EntryType.Error);

            return res.Success;
        }

        public static bool TryGetInfo(string name, IBuffer buffer, out DirectoryInfo dir)
        {
            try
            {
                var fullName = Path.IsPathRooted(name) ? name
                    : Path.Combine(buffer.File?.Directory != null
                        ? buffer.File.Directory.FullName : Environment.CurrentDirectory, name);
                return TryGetInfo(fullName, out dir);
            }
            catch (Exception)
            {
                App.Ext.Log($"Invalid directory name: {name}.", EntryType.Error);
                dir = null;
                return false;
            }
        }

        public static bool TryGetInfo(string name, out DirectoryInfo dir)
        {
            try
            {
                dir = new DirectoryInfo(name);
                return true;
            }
            catch (Exception)
            {
                App.Ext.Log($"Invalid directory name: {name}.", EntryType.Error);
                dir = null;
                return false;
            }
        }

        public static bool TryGetInfo(string name, IBuffer buffer, out FileInfo fileInfo)
        {
            try
            {
                var fullName = Path.IsPathRooted(name) ? name
                    : Path.Combine(buffer.File?.Directory != null
                        ? buffer.File.Directory.FullName : Environment.CurrentDirectory, name);
                return TryGetInfo(fullName, out fileInfo);
            }
            catch (Exception)
            {
                App.Ext.Log($"Invalid file name: {name}.", EntryType.Error);
                fileInfo = null;
                return false;
            }
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
                App.Ext.Log($"Invalid file name: {name}.", EntryType.Error);
                fileInfo = null;
                return false;
            }
        }
    }
}
