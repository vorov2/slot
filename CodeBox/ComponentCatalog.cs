using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace CodeBox
{
    internal static class ComponentCatalog
    {
        public static CompositionContainer CreateContainer(object instance)
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ComponentCatalog).Assembly));
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(instance);
            }
            catch (CompositionException ex)
            {
                throw new CodeBoxException("Composition error.", ex);
            }

            return container;
        }
    }
}
