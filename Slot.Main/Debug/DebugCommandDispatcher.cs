using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Messages;

namespace Slot.Main.Debug
{
    [Export(typeof(ICommandDispatcher))]
    [ComponentData("debug")]
    public sealed class DebugCommandDispatcher : CommandDispatcher
    {
        [Command]
        public void Debug()
        {
            //var res = App.Ext.Show(
            //    "Do you you want to save the changes made to the file strange.txt?"
            //    ,"Some arbitrary text that is just a filler fo for this field to see how the text is going to wrap around the corners of the dialog.\nThe following files:\nfile1.txt\nfile_with_strange.cs\nmarkup.htm\n \nSave them or fuck them?"
            //    ,MessageButtons.Save | MessageButtons.DontSave | MessageButtons.Cancel);
            //MessageBox.Show(res.ToString());
        }

    }
}
