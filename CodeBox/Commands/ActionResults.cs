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
        None = 0x00,

        Clean = 0x01,

        Change = 0x02,

        AtomicChange = 0x04,

        AutocompleteKeep = 0x08,

        AutocompleteShow = 0x10
    }

    public static class ActionResultsExtensions
    {
        public static bool Has(this ActionResults obj, ActionResults flag) => (obj & flag) == flag;
    }
}
