using CodeBox.CommandBar;
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
    [Export(typeof(ICommandDispatcher))]
    [ComponentData("test")]
    public sealed class TestCommandDispatcher : CommandDispatcher
    {
        [Import]
        private IViewManager viewManager = null;

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

            App.Ext.Run(viewManager.GetActiveView(), cmd.Key);
        }

        [Command]
        public void ChangeTheme(string themeName)
        {
            var theme = App.Catalog<IThemeComponent>().First();
            theme.ChangeTheme(themeName);
        }

    }

    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.themes")]
    public sealed class ThemeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var theme = App.Catalog<IThemeComponent>().First();
            return theme.EnumerateThemes()
                .Where(t => str == null || t.Key.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(t => new ValueItem(t.Key, t.Name));
        }
    }

    [Export(typeof(IArgumentValueProvider))]
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

    
}
