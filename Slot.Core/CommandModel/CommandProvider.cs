using Slot.Core.ComponentModel;
using Slot.Core.Keyboard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    [Export(typeof(ICommandProvider))]
    [ComponentData(Name)]
    public sealed class CommandProvider : ICommandProvider
    {
        public const string Name = "commands.default";
        private readonly Dictionary<Identifier, CommandMetadata> commands = new Dictionary<Identifier, CommandMetadata>();
        private readonly Dictionary<string, CommandMetadata> commandsAlias = new Dictionary<string, CommandMetadata>();
        private volatile bool loaded;

        [Import]
        private IViewManager viewManager = null;

        [Import("directory.commands")]
        private string commandsPath = null;

        private void EnsureLoaded()
        {
            if (loaded)
                return;

            var metas = CommandReader.Read(File.ReadAllText(Path.Combine(commandsPath, "commands.json")));
            RegisterCommands(metas);
            loaded = true;
        }

        public void RegisterCommand(CommandMetadata cmd)
        {
            commands.Remove(cmd.Key);
            commands.Add(cmd.Key, cmd);

            if (cmd.Alias != null)
            {
                commandsAlias.Remove(cmd.Alias);
                commandsAlias.Add(cmd.Alias, cmd);
            }
        }

        public void RegisterCommands(IEnumerable<CommandMetadata> cmds)
        {
            foreach (var c in cmds)
            {
                RegisterCommand(c);

                if (c.Shortcut != null)
                    KeyboardAdapter.Instance.RegisterInput(c.Key, c.Shortcut);
            }
        }

        public IEnumerable<CommandMetadata> EnumerateCommands()
        {
            EnsureLoaded();
            var mode = viewManager.GetActiveView()?.Mode;
            return commands
                .Where(p => p.Value.Mode == null
                    || string.Equals(p.Value.Mode, mode, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Value);
        }
        
        public CommandMetadata GetCommandByAlias(string alias)
        {
            EnsureLoaded();
            CommandMetadata ret;
            commandsAlias.TryGetValue(alias, out ret);
            return ret;
        }

        public CommandMetadata GetCommandByKey(Identifier key)
        {
            EnsureLoaded();
            CommandMetadata ret;
            commands.TryGetValue(key, out ret);
            return ret;
        }
    }
}
