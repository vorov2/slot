using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace CodeBox.Core.ComponentModel
{
    public sealed class ComponentCatalog
    {
        private CompositionContainer container;

        [ImportMany]
        private IEnumerable<Lazy<IComponent, IComponentMetadata>> providers = null;
        private Dictionary<string, IComponent> providerMap = new Dictionary<string, IComponent>();

        [ImportMany]
        private IEnumerable<Lazy<ICommand, ICommandMetadata>> commands = null;
        private Dictionary<string, ICommand> commandMap = new Dictionary<string, ICommand>();
        private Dictionary<string, ICommand> commandAliasMap = new Dictionary<string, ICommand>();

        private ComponentCatalog()
        {
            container = CreateContainer();
        }

        private CompositionContainer CreateContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(new FileInfo(typeof(ComponentCatalog).Assembly.Location).DirectoryName));
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                throw new Exception("Composition error.", ex);
            }

            return container;
        }

        public IEnumerable<IComponentMetadata> EnumerateComponents()
        {
            return providers.Select(p => p.Metadata);
        }

        public IEnumerable<ICommandMetadata> EnumerateCommands()
        {
            return commands.Select(p => p.Metadata);
        }

        public IComponent GetComponent(string key)
        {
            IComponent ret;

            if (!providerMap.TryGetValue(key, out ret))
            {
                var comp = providers.FirstOrDefault(c => c.Metadata.Key == key);

                if (comp != null)
                {
                    providerMap.Add(key, comp.Value);
                    ret = comp.Value;
                }
                else
                    return null;
            }

            return ret;
        }

        public ICommand GetCommand(string key)
        {
            ICommand ret;

            if (!commandMap.TryGetValue(key, out ret))
            {
                var cmd = commands.FirstOrDefault(c => c.Metadata.Key == key);

                if (cmd != null)
                {
                    commandMap.Add(key, cmd.Value);
                    ret = cmd.Value;
                }
                else
                    return null;
            }

            return ret;
        }

        public ICommand GetCommandByAlias(string alias)
        {
            ICommand ret;

            if (!commandAliasMap.TryGetValue(alias, out ret))
            {
                var cmd = commands.FirstOrDefault(c => c.Metadata.Alias == alias);

                if (cmd != null)
                {
                    commandAliasMap.Add(alias, cmd.Value);
                    ret = cmd.Value;
                }
                else
                    return null;
            }

            return ret;
        }

        public static ComponentCatalog Instance { get; } = new ComponentCatalog();
    }
}
