using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.BufferCommands
{
    [Export(typeof(IComponent))]
    [ComponentData("buffer")]
    public sealed class BufferCommandDispatcher : CommandDispatcher
    {
        [Import]
        private IViewManager viewManager = null;

        [Command]
        public void SetBufferEol(Eol eol)
        {
            var buf = GetBuffer();

            if (buf != null)
                buf.Eol = eol;
        }

        [Command]
        public void ToggleWordWrap()
        {
            var buf = GetBuffer();

            if (buf != null)
                buf.WordWrap = !buf.WordWrap;
        }

        [Command]
        public void ChangeBufferMode(string mode)
        {
            if (mode != null && ComponentCatalog.Instance.Grammars().GetGrammar(mode) != null)
            {
                var buffer = GetBuffer();

                if (buffer != null)
                    buffer.Mode = mode;
            }
        }

        private DocumentBuffer GetBuffer()
        {
            var view = viewManager.GetActiveView();
            return view?.Buffer as DocumentBuffer;
        }
    }
}
