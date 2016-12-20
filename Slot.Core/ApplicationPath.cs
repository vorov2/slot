using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Slot.Core.ComponentModel;
using StringMacro;
using static System.Configuration.ConfigurationManager;

namespace Slot.Core
{
    [Export(typeof(IComponent))]
    [ComponentData("core.path")]
    public sealed class ApplicationPath : IComponent
    {
        private Dictionary<string, string> macroVariables;

        [Export("directory.packages")]
        public string Packages => ParsePath(AppSettings["directory.packages"]);

        [Export("directory.theme")]
        public string Theme => ParsePath(AppSettings["directory.theme"]);

        [Export("directory.commands")]
        public string Commands => ParsePath(AppSettings["directory.commands"]);

        [Export("directory.grammar")]
        public string Grammar => ParsePath(AppSettings["directory.grammar"]);

        [Export("directory.settings")]
        public string Settings => ParsePath(AppSettings["directory.settings"]);

        [Export("directory.user.settings")]
        public string UserSettings => ParsePath(AppSettings["directory.user.settings"]);

        private string _root;
        [Export("directory.slot")]
        public string Root
        {
            get
            {
                if (_root == null)
                    _root = new FileInfo(typeof(ApplicationPath).Assembly.Location).DirectoryName;
                return _root;
            }
        }

        private string ParsePath(string path)
        {
            if (macroVariables == null)
                macroVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "slot", Root }
                };

            var macro = new MacroParser(macroVariables, VariableProviders.Default);
            return macro.Parse(path);
        }
    }
}
