﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Drawing;
using System.Windows.Forms;
using CodeBox.Core.ComponentModel;
using CodeBox.Commands;

namespace CodeBox.Margins
{
    public sealed class TopMargin : Margin
    {
        public TopMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var cs = Editor.Theme.GetStyle(StandardStyle.Default);
            g.FillRectangle(ControlPaint.Dark(cs.BackColor, .05f).Brush(), bounds);
                //ColorTranslator.FromHtml("#161616").Brush(), bounds);//cs.BackColor.Brush(), bounds);
            return true;
        }

        public override int CalculateSize() => (int)Math.Round(Editor.Info.LineHeight * .1);
    }
}
