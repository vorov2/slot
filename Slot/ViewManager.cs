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

        public IView CreateView()
        {
            var act = (ViewForm)ActiveView;
            var frm = new ViewForm();
            frm.Width = act.Width;
            frm.Height = act.Height;
            frm.Show();
            frm.AttachBuffer(act.Buffer);
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

        public IView ActiveView => EnumerateViews().FirstOrDefault();

        public IEnumerable<IView> EnumerateViews() =>
            Application.OpenForms
                .OfType<IView>()
                .OrderByDescending(f => f.LastAccess);

        public void ActivateView(IView view)
        {
            var frm = view as ViewForm;

            if (frm != null)
            {
                frm.Activate();

                if (frm.Workspace != null)
                    Directory.SetCurrentDirectory(frm.Workspace.FullName);
            }
        }
    }
}
