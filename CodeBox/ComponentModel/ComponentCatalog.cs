using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace CodeBox.ComponentModel
{
    internal sealed class ComponentCatalog
    {
        private CompositionContainer container;

        [ImportMany]
        private IEnumerable<Lazy<IComponent, IComponentMetadata>> providers = null;
        private Dictionary<string, IComponent> providerMap = new Dictionary<string, IComponent>();

        private ComponentCatalog()
        {
            container = CreateContainer();
        }

        private CompositionContainer CreateContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ComponentCatalog).Assembly));
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                throw new CodeBoxException("Composition error.", ex);
            }

            return container;
        }

        public T GetComponent<T>(string key) where T : class, IComponent
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
                    return default(T);
            }

            return ret as T;
        }

        public static ComponentCatalog Instance { get; } = new ComponentCatalog();
    }
}
