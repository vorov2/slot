using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Editor;
using Slot.Core;
using Slot.Core.CommandModel;

namespace Slot
{
    [Export(typeof(IViewManager))]
    [ComponentData(Name)]
    public sealed class ViewManager : IViewManager
    {
        public const string Name = "views.default";

        public ViewManager()
        {

        }

        public IView CreateView()
        {
            var act = GetActiveView();
            var frm = new ViewForm();
            frm.Show();
            frm.Workspace = act.Workspace;
            return frm;
        }

        public void CloseView(IView view)
        {
            if (EnumerateViews().Count() == 1)
                App.Close();
            else
            {
                var frm = view as ViewForm;

                if (frm != null)
                {
                    frm.AllowClose = true;
                    frm.Close();
                }
            }
        }

        public IView GetActiveView()
        {
            var frm = Form.ActiveForm as IView;

            if (frm == null)
                frm = Application.OpenForms
                    .OfType<IView>()
                    .OrderByDescending(f => f.Activations)
                    .FirstOrDefault();

            return frm;
        }

        public IEnumerable<IView> EnumerateViews()
        {
            return Application.OpenForms
                .OfType<IView>()
                .OrderByDescending(f => f.Activations);
        }

        public void ActivateView(IView view)
        {
            var editor = view as EditorControl;

            if (editor != null)
            {
                editor.Buffer.LastAccess = DateTime.Now;
                editor.FindForm().Activate();

                if (view.Workspace != null)
                    Directory.SetCurrentDirectory(view.Workspace.FullName);
            }
        }
    }
}
