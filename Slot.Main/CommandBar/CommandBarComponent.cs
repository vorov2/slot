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

namespace Slot.Main.CommandBar
{
    [Export(typeof(ICommandBar))]
    [ComponentData(Name)]
    public sealed class CommandBarComponent : ICommandBar
    {
        public const string Name = "commandbar.default";

        public void Show(IView view) => Show(view, null);

        public void Show(IView view, string commandAlias, params object[] args)
        {
            var cm = GetCommandBarControl();

            if (cm != null)
            {
                if (commandAlias != null && args != null && args.Length > 0)
                {
                    var stmt = new Statement(commandAlias);
                    stmt.Arguments.AddRange(args.Select(a => new StatementArgument(a)));
                    cm.ShowInput(stmt);
                }
                else if (commandAlias != null)
                    cm.ShowInput(commandAlias);
                else
                    cm.ShowInput();
            }
        }

        public void ToggleMessage()
        {
            var cm = GetCommandBarControl();

            if (cm != null)
                cm.ToggleTip();
        }

        public void Hide(IView view)
        {
            var cm = GetCommandBarControl();

            if (cm != null)
                cm.CloseInput();
        }

        internal static CommandBarControl GetCommandBarControl()
        {
            return Form.ActiveForm.Controls.OfType<SplitContainer>()
                .FirstOrDefault()
                .Panel1.Controls.OfType<CommandBarControl>().FirstOrDefault();
        }
    }
}
