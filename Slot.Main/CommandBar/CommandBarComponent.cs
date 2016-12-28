using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

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
