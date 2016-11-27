using CodeBox.CommandLine;
using CodeBox.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Keyboard;
using CodeBox.Core.ViewModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    [Export(typeof(IComponent))]
    [ComponentData("test")]
    public sealed class TestCommandDispatcher : CommandDispatcher
    {
        [Import("viewmanager.default", typeof(IComponent))]
        private IViewManager viewManager = null;

        [Import("buffermanager.default", typeof(IComponent))]
        private IBufferManager bufferManager = null;

        protected override void ProcessNotEnoughArguments(IExecutionContext ctx, Identifier commandKey, object[] args)
        {
            var ed = ctx as Editor;

            if (ed != null)
            {
                if (ed.Parent is Editor)
                    ed = ed.Parent as Editor;

                var cm = ed.TopMargins.FirstOrDefault(b => b is CommandMargin) as CommandMargin;

                if (cm != null)
                {
                    var cmd = CommandCatalog.Instance.GetCommandByKey(commandKey);

                    if (args == null || args.Length == 0)
                        cm.Toggle(cmd.Alias);
                    else
                    {
                        var stmt = new Statement(cmd.Alias);
                        stmt.Arguments.AddRange(args.Select(a => new StatementArgument(a)));
                        cm.Toggle(stmt);
                    }
                }
            }
        }

        [Command]
        public void CommandPalette(string commandName)
        {
            var cmd = CommandCatalog.Instance.EnumerateCommands()
                .FirstOrDefault(c => c.Title.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            if (cmd == null)
            {
                //Log
                return;
            }

            var exec = ComponentCatalog.Instance.GetComponent(cmd.Key.Namespace) as ICommandDispatcher;
            if (exec != null)
                exec.Execute((IExecutionContext)viewManager.GetActiveView(), cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            theme.ChangeTheme(themeName);
        }

        [Command]
        public void OpenFile(string fileName, Encoding enc = null)
        {
            try
            {
                var fi = new FileInfo(Uri.UnescapeDataString(fileName));
                var buffer = bufferManager.CreateBuffer(fi, enc ?? Encoding.UTF8);
                viewManager.CreateView(buffer);
            }
            catch (Exception)
            {
                //logging
            }
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.themes")]
    public sealed class ThemeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            return theme.EnumerateThemes()
                .Where(t => str == null || t.Key.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(t => new ValueItem(t.Key, t.Name));
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.encoding")]
    public sealed class EncodingValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;

            return Encoding.GetEncodings()
                .Where(e => str == null || e.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(e => new ValueItem(e.Name, e.DisplayName))
                .OrderBy(e => e.Value);
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.commands")]
    public sealed class CommandsProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var strings = (curvalue as string ?? "")
                .Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return CommandCatalog.Instance.EnumerateCommands()
                .Where(c => c.Alias != "?")
                .Where(c => c.Title.ContainsAll(strings))
                .Select(c => new CommandArgumentValue(c, KeyboardAdapter.Instance.GetCommandShortcut(c.Key)));
        }

        class CommandArgumentValue : ValueItem
        {
            private readonly CommandMetadata meta;
            private readonly KeyInput shortcut;

            internal CommandArgumentValue(CommandMetadata meta, KeyInput shortcut)
            {
                this.meta = meta;
                this.shortcut = shortcut;
            }

            public override string Value => meta.Title;

            public override string Meta =>
                shortcut != null ? $"{shortcut} ({meta.Alias})" : $"({meta.Alias})";

            public override string ToString() => Value;
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.systempath")]
    public sealed class SystemPathValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string ?? "";

            //if (!string.IsNullOrWhiteSpace(str))
                return GetPathElements(str) ?? Enumerable.Empty<ValueItem>();

            //return Enumerable.Empty<ArgumentValue>();
        }

        private IEnumerable<ValueItem> GetPathElements(string pat)
        {
            if (pat.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return null;

            try
            {
                if (pat.EndsWith("\\") || pat.EndsWith("//"))
                    return Directory.EnumerateFileSystemEntries(pat)
                        .Where(v => v.StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new ValueItem(sy));
                else
                {
                    FileInfo fi;
                    var dir = string.IsNullOrWhiteSpace(pat) || (fi = new FileInfo(pat)).Directory == null
                        ? new DirectoryInfo(Environment.CurrentDirectory) : fi.Directory;

                    var loc = Environment.CurrentDirectory == dir.Name;
                    return dir.EnumerateFileSystemInfos()
                        .Where(v => (loc ? v.Name : v.FullName).StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new ValueItem(sy.FullName));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
