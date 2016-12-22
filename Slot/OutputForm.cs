using Slot.Core;
using Slot.Core.Output;
using Slot.Editor.Margins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Editor;
using Slot.Editor.Styling;
using Slot.Core.Themes;

namespace Slot
{
    public partial class OutputForm : Form
    {
        private EditorControl output;

        public OutputForm()
        {
            InitializeComponent();
            InitializeOutput();
        }

        private void InitializeOutput()
        {
            output = new EditorControl { Dock = DockStyle.Fill };
            output.UseSmallFont = true;
            output.LeftMargins.Add(new GutterMargin(output));
            output.RightMargins.Add(new ScrollBarMargin(output, Orientation.Vertical));
            output.BottomMargins.Add(new ScrollBarMargin(output, Orientation.Horizontal));
            output.ThinCaret = true;
            output.Styles.StyleNeeded += OutputStyleNeeded;

            //output.Settings.Font = set.SmallFont;
            //output.Settings.ShowEol = output.Settings.ShowLineLength = false;
            //output.Settings.ShowWhitespace = ShowWhitespace.None;
            output.LimitedMode = true;
            output.AttachBuffer(App.Catalog<ILogComponent>().GetComponent((Identifier)"log.application"));
            output.ReadOnly = true;

            Controls.Add(output);
        }

        private void OutputStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            for (var i = e.Range.Start.Line; i < e.Range.End.Line + 1; i++)
            {
                var ln = output.Document.GetLine(i);
                var st = ln.State;
                var style = st == (int)EntryType.Error ? StandardStyle.Error 
                    : st == (int)EntryType.Warning ? StandardStyle.Warning
                    : StandardStyle.Default;
                output.Styles.StyleRange(style, i, 0, ln.Length);
            }

            Console.WriteLine(e.Range);
        }
    }
}
