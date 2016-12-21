using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core
{
    public sealed class UTF8EncodingNoBom : UTF8Encoding
    {
        public UTF8EncodingNoBom() : base(false)
        {

        }

        public override string WebName => $"{base.WebName} No BOM";
    }
}
