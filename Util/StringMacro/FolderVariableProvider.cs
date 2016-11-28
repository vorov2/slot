using System;

namespace StringMacro
{
    public sealed class FolderVariableProvider : IVariableProvider
    {
        public static readonly FolderVariableProvider Instance = new FolderVariableProvider();
        public static readonly string Prefix = "folder";

        private FolderVariableProvider()
        {

        }

        public bool TryResolve(string key, out string value)
        {
            key = key.ToUpper();
            value = null;
            var retval = true;

            switch (key)
            {
                case "DESKTOP":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    break;
                case "PERSONAL":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    break;
                case "MUSIC":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    break;
                case "APPLICATIONDATA":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case "LOCALAPPLICATIONDATA":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    break;
                case "COMMONAPPLICATIONDATA":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    break;
                case "PICTURES":
                    value = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    break;
                default:
                    retval = false;
                    break;
            }

            return retval;
        }
    }
}