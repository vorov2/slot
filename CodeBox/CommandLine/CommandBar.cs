using CodeBox.Core.CommandModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Core.ViewModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.CommandLine
{
    [Export(Name, typeof(IComponent))]
    [ComponentData(Name)]
    public sealed class CommandBar : ICommandBar
    {
        public const string Name = "commandbar.default";

        public void Show(IExecutionContext view) => Show(view, null);

        public void Show(IExecutionContext view, string commandAlias, params object[] args)
        {
            var cm = GetMargin(view);

            if (cm != null)
            {
                if (commandAlias != null && args != null && args.Length > 0)
                {
                    var stmt = new Statement(commandAlias);
                    stmt.Arguments.AddRange(args.Select(a => new StatementArgument(a)));
                    cm.Show(stmt);
                }
                else if (commandAlias != null)
                    cm.Show(commandAlias);
                else
                    cm.Show();
            }
        }

        public void Hide(IExecutionContext view)
        {
            var cm = GetMargin(view);

            if (cm != null)
                cm.Close();
        }

        private CommandMargin GetMargin(IExecutionContext view)
        {
            var editor = view as Editor;

            if (editor == null)
                return null; //Log

            return editor.TopMargins.FirstOrDefault(b => b is CommandMargin) as CommandMargin;
        }
    }
}
