using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Settings;
using Slot.Core.ViewModel;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.BufferCommands
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData(Name)]
    public sealed class BufferCommandDispatcher : CommandDispatcher
    {
        public const string Name = "buffer";

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

        [Command]
        public void ToggleOvertype()
        {
            var buf = GetBuffer();

            if (buf != null)
                buf.Overtype = !buf.Overtype;
        }

        private DocumentBuffer GetBuffer()
        {
            var view = ViewManager.GetActiveView();
            return view?.Buffer as DocumentBuffer;
        }
    }
}
