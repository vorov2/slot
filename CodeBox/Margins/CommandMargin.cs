using System;
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
    public class CommandMargin : Margin
    {
        private Editor commandEditor;
        private Rectangle lastBounds;

        public CommandMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var ed = GetEditor();

            if (ed.Width != Editor.Info.TextWidth)
                HideEditor();

            var cs = Editor.Styles.Styles.GetStyle(StandardStyle.CommandBar);
            g.FillRectangle(cs.BackColor.Brush(), bounds);
            lastBounds = bounds;

            if (!ed.Visible)
            {
                g.DrawString(@"• c:\directory\another directory\some test file.htm",
                    Editor.Settings.SmallFont.Get(cs.FontStyle),
                    cs.ForeColor.Brush(),
                    Editor.Info.TextLeft,
                    (bounds.Height - Editor.Info.SmallCharHeight) / 2);
            }

            return true;
        }

        public override MarginEffects MouseDown(Point loc)
        {
            ShowEditor();
            return MarginEffects.CaptureMouse;
        }

        public override MarginEffects MouseUp(Point loc)
        {
            GetEditor().Focus();
            return MarginEffects.None;
        }

        private Editor GetEditor()
        {
            if (commandEditor == null)
            {
                commandEditor = new Editor(Editor.Settings, Editor.Styles.Styles, Editor.KeyboardAdapter);
                commandEditor.LimitedMode = true;
                commandEditor.Visible = false;
                commandEditor.Height = Editor.Info.LineHeight;
                commandEditor.LostFocus += EditorLostFocus;
                commandEditor.KeyDown += EditorKeyDown;
                ResetBuffer();
                Editor.Controls.Add(commandEditor);
            }

            return commandEditor;
        }

        private void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                var cl = commandEditor.Text;
                HideEditor();
                ExecuteCommand(cl);
            }
            else if (e.KeyData == Keys.Escape)
                HideEditor();
        }

        private void ResetBuffer()
        {
            commandEditor.Text = "";
            commandEditor.Buffer.CurrentLineIndicator = false;
            commandEditor.Buffer.ShowEol = false;
            commandEditor.Buffer.ShowLineLength = false;
            commandEditor.Buffer.ShowWhitespace = false;
            commandEditor.Buffer.WordWrap = false;
        }

        private void EditorLostFocus(object sender, EventArgs e)
        {
            HideEditor();
        }

        public void Toggle()
        {
            if (commandEditor == null || !commandEditor.Visible)
            {
                ShowEditor();
                commandEditor.Focus();
            }
            else
                HideEditor();
        }

        private void ShowEditor()
        {
            var ed = GetEditor();
            ed.Top = (lastBounds.Height - ed.Height) / 2;
            ed.Left = Editor.Info.TextLeft;
            ed.Width = Editor.Info.TextWidth;
            ed.Visible = true;
        }

        private void HideEditor()
        {
            if (commandEditor != null)
            {
                ResetBuffer();
                commandEditor.Visible = false;
            }
        }

        private void ExecuteCommand(string command)
        {
            var cmd = ComponentCatalog.Instance.GetComponent<EditorCommand>(command);

            if (cmd != null)
                cmd.Run(Editor);
        }

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}
