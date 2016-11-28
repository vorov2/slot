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
    [Export(Name, typeof(IComponent))]
    [ComponentData(Name)]
    public sealed class ViewManager : IViewManager
    {
        public const string Name = "viewmanager.default";

        public IView CreateView()
        {
            var frm = new MainForm();
            frm.Show();
            return frm.Editor;
        }

        public IView GetActiveView()
        {
            var frm = (MainForm)Form.ActiveForm;
            return frm.Editor;
        }
    }
}
