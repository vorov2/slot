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
        private Dictionary<Identifier, IComponent> providerMap = new Dictionary<Identifier, IComponent>();

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

        public IComponent GetComponent(Identifier key)
        {
            IComponent ret;

            if (!providerMap.TryGetValue(key, out ret))
            {
                var strKey = key.ToString();
                var comp = providers.FirstOrDefault(c => c.Metadata.Key == strKey);

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

        public static ComponentCatalog Instance { get; } = new ComponentCatalog();
    }
}
