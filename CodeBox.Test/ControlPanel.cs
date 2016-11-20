using CodeBox.Drawing;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    public sealed class ControlPanel : Control
    {
        public ControlPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.FixedHeight | ControlStyles.Selectable, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var bc = CommandEditor?.Styles.Styles.GetStyle(StandardStyle.Popup).BackColor;

            if (bc != null)
                e.Graphics.FillRectangle(bc.Value.Brush(), e.ClipRectangle);

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetHeight();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            var ce = CommandEditor;

            if (ce != null)
            {
                ce.Visible = true;
                ce.Dock = DockStyle.Fill;
                ce.Focus();
            }
        }

        private void SetHeight()
        {
            var lh = CommandEditor?.Info.LineHeight;

            if (lh != null)
                Size = new Size(Width, lh.Value);
        }

        private Editor _commandEditor;
        [Browsable(false)]
        public Editor CommandEditor
        {
            get { return _commandEditor; }
            set
            {
                if (value == _commandEditor)
                    return;

                if (_commandEditor != null)
                {
                    value.LostFocus -= EditorLostFocus;
                    Controls.Remove(_commandEditor);
                    _commandEditor.Dispose();
                }

                value.LostFocus += EditorLostFocus;
                value.Visible = false;
                _commandEditor = value;
                InitializeEditor(value);
                Controls.Add(_commandEditor);
            }
        }

        private void EditorLostFocus(object sender, EventArgs e)
        {
            CommandEditor.Visible = false;
        }

        private void InitializeEditor(Editor ed)
        {
            ed.Buffer.CurrentLineIndicator = false;
            ed.Buffer.ShowEol = false;
            ed.Buffer.ShowLineLength = false;
            ed.Buffer.ShowWhitespace = false;
            ed.Buffer.WordWrap = false;
            ed.LimitedMode = true;
        }
    }
}
