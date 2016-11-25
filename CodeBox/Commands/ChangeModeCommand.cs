using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Core.CommandModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.changemode")]
    public sealed class ChangeModeCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var mode = GetArg<string>(0, args);

            if (mode != null)
            {
                if (ComponentCatalog.Instance.Grammars().GetGrammar(mode) != null)
                {
                    View.Buffer.GrammarKey = mode;
                    View.Styles.RestyleDocument();
                }
            }

            return ActionResults.Clean;
        }

        internal override bool SingleRun { get; }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.modes")]
    public sealed class ModeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = (curvalue ?? "").ToString();
            return ComponentCatalog.Instance.Grammars().EnumerateGrammars()
                .Where(g => g.Key.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(g => new ValueItem(g.Key, g.Name));
        }
    }
}
