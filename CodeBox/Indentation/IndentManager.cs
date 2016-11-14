using CodeBox.Indentation;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Affinity;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace CodeBox.Indentation
{
    public sealed class IndentManager
    {
        private readonly Editor editor;
        private CompositionContainer container;

        [ImportMany]
        private IEnumerable<Lazy<IDentProvider, IComponentMetadata>> providers = null;
        private Dictionary<string, IDentProvider> providerMap = new Dictionary<string, IDentProvider>();


        internal IndentManager(Editor editor)
        {
            this.editor = editor;
            container = ComponentCatalog.CreateContainer(this);
        }

        public int CalculateIndentation(int lineIndex)
        {
            var pos = new Pos(lineIndex, 0);
            var provKey = editor.AffinityManager.GetAffinity(pos).GetIndentProvider(editor, pos);

            if (provKey == null)
                return 0;

            var prov = default(IDentProvider);

            if (!providerMap.TryGetValue(provKey, out prov))
            {
                var provInfo = providers.FirstOrDefault(o => o.Metadata.Key == provKey);

                if (provInfo != null)
                    prov = provInfo.Value;

                providerMap.Add(provKey, prov);
            }

            return prov != null ? prov.Calculate(lineIndex, editor) : 0;
        }
    }
}
