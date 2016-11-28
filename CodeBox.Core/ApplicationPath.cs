﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using CodeBox.Core.ComponentModel;
using static System.Configuration.ConfigurationManager;

namespace CodeBox.Core
{
    [Export(typeof(IComponent))]
    [ComponentData("core.path")]
    public sealed class ApplicationPath : IComponent
    {
        [Export("directory.theme")]
        public string Theme => AppSettings["directory.theme"];

        [Export("directory.grammar")]
        public string Grammar => AppSettings["directory.grammar"];

        private string _root;
        [Export("directory.root")]
        public string Root
        {
            get
            {
                if (_root == null)
                    _root = new FileInfo(typeof(ApplicationPath).Assembly.Location).DirectoryName;
                return _root;
            }
        }
    }
}