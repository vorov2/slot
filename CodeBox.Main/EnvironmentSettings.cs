using System;
using System.Drawing;
using CodeBox.Core.Settings;

namespace CodeBox.Main
{
    public class EnvironmentSettings : SettingsBag
    {
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
    }
}
