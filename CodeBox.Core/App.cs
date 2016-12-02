using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Core
{
    public sealed class App
    {
        [Import]
        private IViewManager viewManager = null;

        [Import]
        private IBufferManager bufferManager = null;

        private App()
        {

        }

        public bool Close()
        {
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
                    var exec = ComponentCatalog.Instance.GetComponent(cmd.Namespace) as ICommandDispatcher;

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

        public bool Terminating { get; private set; }

        public static App Instance { get; } = new App();
    }
}
