using CodeBox.CommandLine;
using CodeBox.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Keyboard;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox
{
    [Export(typeof(IComponent))]
    [ComponentData("test")]
    public sealed class TestCommandDispatcher : CommandDispatcher
    {
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
                        var stmt = new Statement { Command = cmd.Alias };
                        stmt.Arguments.AddRange(args.Select(a => new StatementArgument
                        {
                            Value = a
                        }));
                        cm.Toggle(stmt);
                    }
                }
            }
        }

        [Command]
        public void CommandPalette(string commandName)
        {
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            theme.ChangeTheme(themeName);
        }

        [Command]
        public void OpenFile(string fileName, Encoding enc)// = "utf-8")
        {
            try
            {
                var txt = File.ReadAllText(Uri.UnescapeDataString(fileName), enc);
                //((Editor)ctx).AttachBuffer(new DocumentBuffer(Document.FromString(txt), fileName, enc));
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
