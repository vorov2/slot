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
        private IEnumerable<Lazy<ICommandComponent, ICommandComponentMetadata>> commands = null;
        private Dictionary<string, ICommandComponent> commandMap = new Dictionary<string, ICommandComponent>();
        private Dictionary<string, ICommandComponent> commandAliasMap = new Dictionary<string, ICommandComponent>();

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

        public IEnumerable<ICommandComponentMetadata> EnumerateCommands()
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

        public ICommandComponent GetCommand(string key)
        {
            ICommandComponent ret;

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

        public ICommandComponent GetCommandByAlias(string alias)
        {
            ICommandComponent ret;

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
