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
    public static partial class App
    {
        public readonly static bool IsMono = Type.GetType("Mono.Runtime") != null;
        
        #region Info
        public const string Product = "Slot";
        public const string Company = "Vasily Voronkov";
        public const string Description = "Slot, a next generation text editor";
        public const string Copyright = "Copyright © Vasily Voronkov 2016";
        #if PublicRelease
        public const string Configuration = "Public";
        #else
        public const string Configuration = "Insiders";
        #endif
        #endregion

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
            var ev = new ExitEventArgs();
            OnExit(ev);

            if (ev.Cancel)
                return false;

            Terminating = true;
            Application.Exit();
            return true;
        }

        public static event EventHandler<ExitEventArgs> Exit;
        private static void OnExit(ExitEventArgs e) => Exit?.Invoke(null, e);

        public static bool Terminating { get; private set; }

        public static IAppExtensions Ext { get; } = null;
    }

    public sealed class ExitEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    public interface IAppExtensions
    {

    }
}
