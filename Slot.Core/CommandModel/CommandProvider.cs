using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Json;
using Slot.Core.ComponentModel;
using Slot.Core.Keyboard;
using Slot.Core.Packages;
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

        private void EnsureLoaded()
        {
            if (loaded)
                return;

            foreach (var pkg in App.Catalog<IPackageManager>().Default().EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Commands))
                {
                    var name = e.String("file");
                    string content;

                    if (!FileUtil.ReadFile(Path.Combine(pkg.Directory.FullName, "data", name), Encoding.UTF8, out content))
                        return;

                    var metas = CommandReader.Read(content);
                    RegisterCommands(metas);
                }

            
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
            var km = App.Catalog<IKeyboardAdapter>().Default();

            foreach (var c in cmds)
            {
                RegisterCommand(c);

                if (c.Shortcut != null)
                    km.RegisterInput(c.Key, c.Shortcut);
            }
        }

        public IEnumerable<CommandMetadata> EnumerateCommands()
        {
            EnsureLoaded();
            var mode = viewManager.ActiveView?.Mode;
            return commands
                .Where(p => p.Value.Mode == null || p.Value.Mode == mode)
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

            if (ret == null)
                throw new SlotException($"Unknown command: {key}!");

            return ret;
        }
    }
}
