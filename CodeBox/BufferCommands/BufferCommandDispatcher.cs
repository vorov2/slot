using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Settings;
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
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class BufferCommandDispatcher : CommandDispatcher
    {
        public const string Name = "buffer";

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
            {
                var val = buf.WordWrap != null ? buf.WordWrap.Value
                    : App.Catalog<ISettingsProvider>().Default().Get<EditorSettings>().WordWrap;
                buf.WordWrap = !val;
            }
        }

        [Command]
        public void ToggleReadOnly()
        {
            var buf = GetBuffer();

            if (buf != null)
                buf.ReadOnly = !buf.ReadOnly;
        }

        private DocumentBuffer GetBuffer()
        {
            var view = viewManager.GetActiveView();
            return view?.Buffer as DocumentBuffer;
        }
    }
}
