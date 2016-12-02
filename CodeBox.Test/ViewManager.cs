using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    [Export(typeof(IViewManager))]
    public sealed class ViewManager : IViewManager
    {
        public IView CreateView()
        {
            var frm = new MainForm();
            frm.Show();
            return frm.Editor;
        }

        public IView GetActiveView()
        {
            var frm = Form.ActiveForm as MainForm;

            if (frm == null)
                frm = Application.OpenForms
                    .OfType<MainForm>()
                    .OrderByDescending(f => f.Activations)
                    .FirstOrDefault();

            return frm?.Editor;
        }

        public IEnumerable<IView> EnumerateViews()
        {
            return Application.OpenForms
                .OfType<MainForm>()
                .OrderByDescending(f => f.Activations)
                .Distinct()
                .Select(f => f.Editor);
        }

        public void ActivateView(IView view)
        {
            var editor = view as Editor;

            if (editor != null)
            {
                editor.Buffer.LastAccess = DateTime.Now;
                editor.FindForm().Activate();
            }
        }
    }
}
