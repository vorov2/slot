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
                exec.Execute(viewManager.GetActiveView(), cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var theme = ComponentCatalog.Instance.GetComponent((Identifier)"theme.default") as IThemeComponent;
            theme.ChangeTheme(themeName);
        }

        [Command]
        public void NewWindow()
        {
            var act = viewManager.GetActiveView();
            var view = viewManager.CreateView();
            view.AttachBuffer(act.Buffer);
        }

        [Command]
        public void OpenFile(string fileName, Encoding enc = null)
        {
            var fi = new FileInfo(Uri.UnescapeDataString(fileName));
            var buffer = bufferManager.CreateBuffer(fi, enc ?? Encoding.UTF8);
            OpenBuffer(buffer);
        }

        [Command]
        public void NewFile()
        {
            var buffer = bufferManager.CreateBuffer();
            OpenBuffer(buffer);
        }

        [Command]
        public void SaveFile(string fileName = null)
        {
            var view = viewManager.GetActiveView();
            var buffer = view.Buffer as IMaterialBuffer;

            if (buffer == null)
            {
                //log
                //basically impossible situation
                return;
            }

            var fi = fileName != null
                ? new FileInfo(Path.Combine(buffer.File?.Directory != null 
                    ? buffer.File.Directory.FullName : Environment.CurrentDirectory, fileName))
                : buffer.File;
            bufferManager.SaveBuffer(buffer, fi, buffer.Encoding);
        }

        [Command]
        public void OpenRecentFile(string fileName)
        {
            var buf = bufferManager.EnumerateBuffers()
                .FirstOrDefault(b => b.File.Name.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            OpenBuffer(buf);
        }

        private void OpenBuffer(IBuffer buf)
        {
            if (buf != null)
            {
                var view = viewManager.GetActiveView();
                view.AttachBuffer(buf);

                if (buf?.File.Directory != null)
                    Directory.SetCurrentDirectory(buf.File.Directory.FullName);
            }
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.recentdocs")]
    public sealed class RecentDocsValueProvider : IArgumentValueProvider
    {
        [Import("viewmanager.default", typeof(IComponent))]
        private IViewManager viewManager = null;

        [Import("buffermanager.default", typeof(IComponent))]
        private IBufferManager bufferManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var cur = viewManager.GetActiveView().Buffer;
            return bufferManager.EnumerateBuffers()
                .Where(b => b != cur && (str == null || b.File.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1))
                .OrderByDescending(b => b.LastAccess)
                .Select(b => new ValueItem(b.File.Name, b.File.DirectoryName));
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
                shortcut != null ? $"{shortcut} ({meta.Alias})" : $"{meta.Alias}";

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
            return GetPathElements(str) ?? Enumerable.Empty<ValueItem>();
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
                    var loc = Environment.CurrentDirectory == dir.FullName;
                    return dir.EnumerateFileSystemInfos()
                        .Where(v => (loc ? v.Name : v.FullName).StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new ValueItem(loc ? sy.Name : sy.FullName));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
