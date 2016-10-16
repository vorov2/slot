using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    public sealed class EditorSettings
    {
        private const string SEPS = "`~!@#$%^&*()-=+[{]}\\|;:'\",.<>/?";
        private const int TABSIZE = 4;

        public EditorSettings()
        {
            //Defaults
            WordSeparators = SEPS;
            UseTabs = false;
            TabSize = 4;
            LinePadding = .1;
            ShowWhitespace = true;
        }

        public string WordSeparators { get; set; }

        public bool WordWrap { get; set; }

        public Eol Eol { get; set; }

        public bool UseTabs { get; set; }

        public int TabSize { get; set; }

        public double LinePadding { get; set; }

        public bool ShowEol { get; set; }

        public bool ShowWhitespace { get; set; }
    }

    public enum Eol
    {
        Auto = 0,

        Cr,

        Lf,

        CrLf
    }
}
