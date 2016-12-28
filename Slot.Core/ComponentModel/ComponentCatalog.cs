using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Slot.Core.ComponentModel
{
    public sealed class ComponentCatalog<T> where T : IComponent
    {
        [ImportMany]
        private IEnumerable<Lazy<T, IComponentMetadata>> components = null;

        private Dictionary<Identifier, T> componentMap = new Dictionary<Identifier, T>();

        public ComponentCatalog()
        {

        }

        public IEnumerable<IComponentMetadata> EnumerateComponents()
        {
            return components.Select(p => p.Metadata);
        }

        public T GetComponent(Identifier key)
        {
            T ret;

            if (!componentMap.TryGetValue(key, out ret))
            {
                var strKey = key.ToString();
                var comp = components.FirstOrDefault(c => c.Metadata.Key == strKey);

                if (comp != null)
                {
                    componentMap.Add(key, comp.Value);
                    ret = comp.Value;
                }
                else
                    return default(T);
            }

            return ret;
        }

        internal T Default()
        {
            var ret = componentMap.Values.FirstOrDefault();

            if (ret == null)
            {
                var cmp = components.FirstOrDefault();

                if (cmp != null)
                {
                    ret = cmp.Value;
                    componentMap.Add((Identifier)cmp.Metadata.Key, cmp.Value);
                }
            }

            if (ret == null)
                throw new Exception($"No component of type {typeof(T).Name} is registered.");

            return ret;
        }
    }
}
