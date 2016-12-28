using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Output;
using Slot.Core.State;

namespace Slot
{
    partial class ViewForm
    {
        private readonly static Guid stateId = new Guid("91B8C085-429A-4460-94FB-8B320DFB0DDA");
        private readonly int BIN_VERSION = 1;

        private bool ReadState()
        {
            var res = App.Ext.Handle(() =>
            {
                var stream = App.Component<IStateManager>().ReadState(stateId);

                if (stream != null)
                    using (var br = new BinaryReader(stream))
                    {
                        if (br.ReadInt32() != BIN_VERSION)
                        {
                            App.Ext.Log($"Invalid version of view state {stateId}. View state not restored.", EntryType.Error);
                            throw new Exception();
                        }

                        Top = br.ReadInt32();
                        Left = br.ReadInt32();
                        Width = br.ReadInt32();
                        Height = br.ReadInt32();
                    }
            });
            return res.Success;
        }

        private void WriteState()
        {
            if (WindowState != FormWindowState.Normal)
                return;

            App.Ext.Handle(() =>
            {
                var stream = App.Component<IStateManager>().WriteState(stateId);

                if (stream != null)
                    using (var bw = new BinaryWriter(stream))
                    {
                        bw.Write(BIN_VERSION);
                        bw.Write(Top);
                        bw.Write(Left);
                        bw.Write(Width);
                        bw.Write(Height);
                    }
            });
        }
    }
}
