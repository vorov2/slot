using Slot.Core.CommandModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Core.ViewModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Slot.Core;

namespace Slot.Main.CommandBar
{
    [Export(typeof(ICommandBar))]
    [ComponentData(Name)]
    public sealed class CommandBarComponent : ICommandBar
    {
        public const string Name = "commandbar.default";

        public void Show() => Show(null);

        public void Show(string commandAlias, params object[] args)
        {
            var cm = GetCommandBarControl();

            if (cm != null)
            {
                if (commandAlias != null && args != null && args.Length > 0)
                {
                    var stmt = new Statement(commandAlias);
                    stmt.Arguments.AddRange(args.Select(a => new StatementArgument(a)));
                    cm.OpenInput(stmt);
                }
                else if (commandAlias != null)
                    cm.OpenInput(commandAlias);
                else
                    cm.OpenInput();
            }
        }

        public void ToggleMessage()
        {
            //var cm = GetCommandBarControl();

            //if (cm != null)
            //    cm.ToggleTip();
        }

        public void Hide()
        {
            var cm = GetCommandBarControl();

            if (cm != null)
                cm.CloseInput();
        }

        public bool InputVisible => GetCommandBarControl()?.IsActive ?? false;

        internal static CommandBarControl GetCommandBarControl()
        {
            return (CommandBarControl)App.Component<IViewManager>().ActiveView.CommandBar;
        }
    }
}
