using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Core
{
    public static class App
    {
        private static readonly Dictionary<Type, object> catalogs = new Dictionary<Type, object>();
        private static CompositionContainer container;

        public static void Initialize()
        {
            var catalog = new AggregateCatalog();
            var path = new FileInfo(typeof(App).Assembly.Location).DirectoryName;
            catalog.Catalogs.Add(new DirectoryCatalog(path, "*.dll"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "*.exe"));
            container = new CompositionContainer(catalog);

            foreach (var c in catalogs.Values)
                container.ComposeParts(c);
        }

        public static void RegisterCatalog<T>() where T : IComponent
        {
            var type = typeof(T);

            if (catalogs.ContainsKey(type))
                throw new Exception($"Component catalog for '{type.Name}' is already registered.");

            var cat = new ComponentCatalog<T>();
            catalogs.Add(type, cat);
        }

        public static ComponentCatalog<T> Catalog<T>() where T : IComponent
        {
            object ret;

            if (!catalogs.TryGetValue(typeof(T), out ret))
                throw new Exception($"Unable to find component catalog for '{typeof(T).Name}'.");

            return (ComponentCatalog<T>)ret;
        }

        public static bool Close()
        {
            var bufferManager = Catalog<IBufferManager>().Default();
            var seq = bufferManager.EnumerateBuffers()
                .OfType<IMaterialBuffer>()
                .Where(b => b.IsDirty);

            if (seq.Any())
            {
                var sb = new StringBuilder();
                var res = MessageBox.Show(Application.OpenForms[0],
                    $"Do you want to save the changes to the following files?\n\n{string.Join("\n", seq.Select(f => f.File.Name))}",
                    Application.ProductName,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (res == DialogResult.Yes)
                {
                    var cmd = (Identifier)"file.save";
                    var exec = Catalog<ICommandDispatcher>().GetComponent(cmd.Namespace);

                    foreach (var d in seq)
                        exec.Execute(null, cmd, d.File.FullName);
                }

                if (res == DialogResult.Cancel)
                    return false;
            }

            Terminating = true;
            Application.Exit();
            return true;
        }

        public static bool Terminating { get; private set; }

        public static IAppExtensions Ext { get; } = null;
    }

    public interface IAppExtensions
    {

    }
}
