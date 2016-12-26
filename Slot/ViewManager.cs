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

namespace Slot
{
    [Export(typeof(IViewManager))]
    [ComponentData(Name)]
    public sealed class ViewManager : IViewManager
    {
        public const string Name = "views.default";

        public IView CreateView()
        {
            var act = GetActiveView();
            var frm = new MainForm();
            frm.Show();
            ((IView)frm.Editor).Workspace = act.Workspace;
            frm.UpdateTitle();
            return frm.Editor;
        }

        public void CloseView(IView view)
        {
            var ed = view as EditorControl;

            if (ed != null)
                ed.FindForm().Close();
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
