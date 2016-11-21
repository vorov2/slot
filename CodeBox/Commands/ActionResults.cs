using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Commands
{
    [Flags]
    public enum ActionResults
    {
        //Combinations
        Pure = Clean | Silent | IdleCaret,
        Change = Modify | RestoreCaret | Scroll,


        None = 0x00,

        Silent = 0x01,

        RestoreCaret = 0x02,

        Scroll = 0x04,

        NeedUndo = 0x08,

        NeedRedo = 0x10,

        Clean = 0x20,

        Modify = 0x40,

        LeaveEditor = 0x80,

        ShallowChange = 0x100,

        IdleCaret = 0x200,

        AtomicChange = 0x400,

        AutocompleteKeep = 0x800,

        AutocompleteShow = 0x1000,

        KeepRedo = 0x2000,
    }

    public static class ActionResultsExtensions
    {
        public static bool Has(this ActionResults obj, ActionResults flag) => (obj & flag) == flag;
    }
}
