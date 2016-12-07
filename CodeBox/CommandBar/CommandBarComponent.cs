using CodeBox.Core.CommandModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Core.ViewModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace CodeBox.CommandBar
{
    [Export(typeof(ICommandBar))]
    [ComponentData(Name)]
    public sealed class CommandBarComponent : ICommandBar
    {
        public const string Name = "commandbar.default";

        public void Show(IExecutionContext view) => Show(view, null);

        public void Show(IExecutionContext view, string commandAlias, params object[] args)
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

        public void Hide(IExecutionContext view)
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
