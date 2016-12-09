using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Drawing
{
    public static class Dpi
    {
        private static float? dpiX;
        private static float? dpiY;

        public static int GetWidth(int baseWidth)
        {
            FillDpi();
            return (int)Math.Round(baseWidth * dpiX.Value, MidpointRounding.AwayFromZero);
        }

        public static int GetHeight(int baseHeight)
        {
            FillDpi();
            return (int)Math.Round(baseHeight * dpiY.Value, MidpointRounding.AwayFromZero);
        }

        private static void FillDpi()
        {
            if (dpiX == null)
            {
                using (var ctl = new Control())
                using (var g = ctl.CreateGraphics())
                {
                    dpiX = g.DpiX / 96f;
                    dpiY = g.DpiY / 96f;
                }
            }
        }
    }
}
