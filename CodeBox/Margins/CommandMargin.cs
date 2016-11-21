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
using CodeBox.CommandLine;

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
                commandEditor.Paint += EditorPaint;
                commandEditor.Styles.StyleNeeded += EditorStyleNeeded;
                ResetBuffer();
                Editor.Controls.Add(commandEditor);
            }

            return commandEditor;
        }

        private IEnumerable<Statement> statements;
        private void EditorStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            statements = CommandParser.Parse(commandEditor.Text);
            commandEditor.Styles.ClearStyles(0);

            foreach (var s in statements)
            {
                commandEditor.Styles.StyleRange(StandardStyle.Keyword, 0, s.Location.Start, s.Location.End);

                if (s.HasArguments)
                {
                    foreach (var a in s.Arguments)
                    {
                        var st = a.Value is string ? StandardStyle.String : StandardStyle.Number;
                        commandEditor.Styles.StyleRange(st, 0, a.Location.Start, a.Location.End);
                    }
                }
            }
        }

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statements != null && statements.Any())
            {
                var stmt = statements.Last();

                if (!stmt.HasArguments && !string.IsNullOrWhiteSpace(stmt.Command))
                {
                    var tetras = commandEditor.Lines[0].GetTetras(commandEditor.IndentSize);
                    var w = (tetras + 1) * commandEditor.Info.CharWidth;
                    var cmd = stmt.Command;
                    var arr = 
                        ComponentCatalog.Instance.EnumerateCommands()
                        .Where(c => c.Alias.StartsWith(cmd))
                        .ToList();

                    var g = e.Graphics;
                    var st = commandEditor.Styles.Styles.GetStyle(StandardStyle.SpecialSymbol);
                    var cw = commandEditor.Info.CharWidth;

                    if (arr.Count == 0)
                        return;
                    else if (arr.Count > 1)
                    {
                        foreach (var a in arr)
                        {
                            var len = cw * (a.Alias.Length + 1);

                            if (w + len + cw * 3 > commandEditor.Info.TextWidth)
                            {
                                g.DrawString("...", commandEditor.Font, st.ForeColor.Brush(), w, 0);
                                break;
                            }
                            else
                            {
                                g.DrawString(a.Alias, commandEditor.Font, st.ForeColor.Brush(), w, 0);
                                w += len;
                            }
                        }
                    }
                    else
                    {
                        var a = arr[0];
                        g.DrawString(a.Key, commandEditor.Font, st.ForeColor.Brush(), w, 0);
                    }
                }
            }
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
            var cmd = ComponentCatalog.Instance.GetCommandByAlias(command) as EditorCommand;

            if (cmd != null)
                cmd.Run(Editor);
        }

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}
