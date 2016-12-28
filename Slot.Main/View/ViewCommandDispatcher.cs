using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Output;
using Slot.Core.ViewModel;
using Slot.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Main.View
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class ViewCommandDispatcher : Core.CommandModel.CommandDispatcher
    {
        public const string Name = "view";

        [Command]
        public void NewView()
        {
            var act = ViewManager.ActiveView;
            var view = ViewManager.CreateView();
            view.AttachBuffer(act.Buffer);
        }

        [Command]
        public void SwitchView(string viewName)
        {
            var view = ViewManager.EnumerateViews()
                 .FirstOrDefault(v => v.Buffer.File.Name.Contains(viewName));
            ViewManager.ActivateView(view);
        }

        [Command]
        public void CloseView()
        {
            var view = ViewManager.ActiveView;
            ViewManager.CloseView(view);
        }

        [Command]
        public void ToggleViewTopmost()
        {
            var view = ViewManager.ActiveView;
            var frm = ((Control)view).FindForm();
            frm.TopMost = !frm.TopMost;
        }

        [Command]
        public void ToggleViewState()
        {
            var view = ViewManager.ActiveView;
            var frm = ((Control)view).FindForm();

            if (frm.WindowState == FormWindowState.Maximized)
                frm.WindowState = FormWindowState.Normal;
            else
                frm.WindowState = FormWindowState.Maximized;
        }

        [Command]
        public void ToggleFullScreen()
        {
            var view = ViewManager.ActiveView;
            var frm = ((Control)view).FindForm();

            if (frm.FormBorderStyle == FormBorderStyle.None)
            {
                frm.WindowState = FormWindowState.Normal;
                frm.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.WindowState = FormWindowState.Maximized;
            }
        }
    }
}
