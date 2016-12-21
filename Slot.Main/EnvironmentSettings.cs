using System;
using System.Drawing;
using Slot.Core.Settings;

namespace Slot.Main
{
    public class EnvironmentSettings : SettingsBag
    {
        [Setting("keymap")]
        public string Keymap { get; set; }

        [Setting("ui.font")]
        public string FontName { get; set; }

        [Setting("ui.fontSize")]
        public int FontSize { get; set; }

        private Font _font;
        public Font Font
        {
            get
            {
                if (_font == null)
                    _font = new Font(FontName, FontSize);

                return _font;
            }
        }

        private Font _smallFont;
        public Font SmallFont
        {
            get
            {
                if (_smallFont == null)
                    _smallFont = new Font(FontName, FontSize - 1);

                return _smallFont;
            }
        }
    }
}
