using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.ViewModel;

namespace Slot
{
    [Serializable]
    public sealed class ApplicationServer : MarshalByRefObject
    {
        private const string SERVER = "Slot.ApplicationServer";
        private const string INSTANCE = "Slot";
        private const string LOCK = "Slot-19645371-4E1B-4517-9CC9-1D5F5AA618B7";
        private IpcChannel serverChannel;
        private FileStream fileStream;

        private string FileName => Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), LOCK);

        public void OpenView(string fileName = null)
        {
            Application.OpenForms[0].Invoke((MethodInvoker)(() =>
            {
                var view = App.Component<IViewManager>().CreateView();
                var buf = App.Component<IBufferManager>();
                FileInfo fi;

                if (fileName != null && FileUtil.TryGetInfo(fileName, out fi))
                    view.AttachBuffer(buf.CreateBuffer(fi, Encoding.UTF8));
                else
                    view.AttachBuffer(buf.CreateBuffer());

                App.Component<IViewManager>().ActivateView(view);
            }));
        }

        public void StopServer()
        {
            fileStream.Unlock(0, fileStream.Length);
            fileStream.Dispose();
            ChannelServices.UnregisterChannel(serverChannel);
            Process.GetCurrentProcess().Kill();
        }

        public void StartServer()
        {
            fileStream = ObtainLock();
            fileStream.Lock(0, fileStream.Length);
            serverChannel = new IpcChannel(SERVER);
            ChannelServices.RegisterChannel(serverChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ApplicationServer), INSTANCE,
                WellKnownObjectMode.Singleton);
        }

        private FileStream ObtainLock()
        {
            try
            {
                return File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch //TODO
            {
                return null;
            }
        }

        public ApplicationServer ConnectServer()
        {
            var fs = ObtainLock();

            if (fs != null)
            {
                fs.Close();
                return null;
            }

            try
            {
                var obj = Activator.GetObject(typeof(ApplicationServer),
                    $"ipc://{SERVER}/{INSTANCE}") as ApplicationServer;

                var _ = obj.Ping(); //TODO: ugly
                return obj;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Guid Ping()
        {
            return Id;
        }

        public Guid Id { get; } = Guid.NewGuid();
    }
}
