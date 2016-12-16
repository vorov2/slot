using System;

namespace Slot.Core.Themes
{
    public sealed class StyleInfo
    {
        public StyleInfo(StandardStyle styleId, Style style)
        {
            StyleId = styleId;
            Style = style;
        }

        public StandardStyle StyleId { get; }

        public Style Style { get; }
    }
}
