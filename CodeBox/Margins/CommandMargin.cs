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
using CodeBox.Autocomplete;

namespace CodeBox.Margins
{
    public class CommandMargin : Margin
    {
        private Editor commandEditor;
        private AutocompleteWindow window;
        private Rectangle lastBounds;

        public CommandMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var ed = GetEditor();

            if (ed.Width != PreferredEditorWidth)
                HideEditor();

            var cs = Editor.Styles.Styles.GetStyle(StandardStyle.CommandBar);
            g.FillRectangle(cs.BackColor.Brush(), bounds);
            lastBounds = bounds;

            if (!ed.Visible)
            {
                g.DrawString(Editor.Buffer.FileName + (Editor.Buffer.IsDirty ? "*" : ""),
                    Editor.Settings.SmallFont.Get(cs.FontStyle),
                    cs.ForeColor.Brush(),
                    Editor.Info.CharWidth * 2,//Editor.Info.TextLeft,
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
                commandEditor = new Editor(Editor.Settings, Editor.Styles.Styles, Editor.KeyboardAdapter, new Lexing.GrammarManager());
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

        private AutocompleteWindow GetAutocompleteWindow()
        {
            if (window == null)
            {
                window = new AutocompleteWindow(Editor) { MaxItems = 15, SmallFont = true };
                window.Visible = false;
                window.Trimming = StringTrimming.EllipsisPath;
                Editor.Controls.Add(window);
            }

            return window;
        }

        private string lastLookupInput;
        private void ShowAutocompleteWindow(IArgumentValueProvider prov)
        {
            var wnd = GetAutocompleteWindow();
            wnd.PreferredWidth = commandEditor.Width;
            var items = prov.EnumerateArgumentValues(lastLookupInput)
                .Select(v => v.ToString());

            if (items.Any())
            {
                wnd.SetItems(items);
                wnd.Left = commandEditor.Left;
                wnd.Top = lastBounds.Top + lastBounds.Height;
                wnd.Visible = true;
                wnd.MouseDown += AutocompleteClick;
                wnd.Invalidate();
                Editor.LockMouseScrolling = true;
            }
            else
                HideAutocompleteWindow();
        }

        private void AutocompleteClick(object sender, MouseEventArgs e)
        {
            if (window != null && window.Visible)
                InsertCompleteString();
        }

        private bool HideAutocompleteWindow()
        {
            if (window != null && window.Visible)
            {
                window.Visible = false;
                Editor.LockMouseScrolling = false;
                return true;
            }

            return false;
        }

        private Statement statement;
        private void EditorStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            statement = CommandParser.Parse(commandEditor.Text);
            commandEditor.Styles.ClearStyles(0);

            if (statement == null)
                return;

            commandEditor.Styles.StyleRange(StandardStyle.Keyword, 0,
                statement.Location.Start, statement.Location.End);

            if (statement.Argument != null)
            {
                var st = statement.ArgumentType == ArgumentType.String ? StandardStyle.String
                        : statement.ArgumentType == ArgumentType.Number ? StandardStyle.Number
                        : StandardStyle.Default;
                commandEditor.Styles.StyleRange(st, 0,
                    statement.ArgumentLocation.Start, statement.ArgumentLocation.End);
            }
        }

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statement != null)
            {
                var stmt = statement;

                if (!string.IsNullOrWhiteSpace(stmt.Command))
                {
                    var cmd = stmt.Command;
                    var arr =
                        ComponentCatalog.Instance.EnumerateCommands()
                        .Where(c => c.Alias.StartsWith(cmd))
                        .ToList();

                    var g = e.Graphics;
                    
                    if (arr.Count == 0)
                        return;
                    else if (arr.Count > 1 || stmt.Command.Length < arr[0].Alias.Length)
                    {
                        DrawStringWithPeriods(g, string.Join(" ", arr.Select(a => a.Alias)));
                        HideAutocompleteWindow();
                    }
                    else
                    {
                        var a = arr[0];
                        var str = a.Key;

                        if (a.ArgumentType != ArgumentType.None)
                            str += $"({a.ArgumentName}:{a.ArgumentType.ToString().ToLower()})";

                        DrawStringWithPeriods(g, str);
                        var obj = ComponentCatalog.Instance.GetCommandByAlias(a.Alias);
                        var prov = obj as IArgumentValueProvider;

                        if (prov != null)
                        {
                            var arg = stmt.Argument != null ? stmt.Argument.ToString() : "";
                            lastLookupInput = arg;
                            ShowAutocompleteWindow(prov);
                        }
                        else
                            HideAutocompleteWindow();
                    }
                }
            }
            else
                HideAutocompleteWindow();
        }

        private void DrawStringWithPeriods(Graphics g, string str)
        {
            var tetras = commandEditor.Lines[0].GetTetras(commandEditor.IndentSize);
            var cw = commandEditor.Info.SmallCharWidth;
            var font = commandEditor.Settings.SmallFont;
            var x = (tetras + 1) * commandEditor.Info.CharWidth;
            var y = (commandEditor.Height - (int)(commandEditor.Info.SmallCharHeight * 1.1)) / 2;
            var brush = commandEditor.Styles.Styles.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Brush();

            if (x + cw * 2 >= commandEditor.Info.TextWidth - cw)
                return;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                g.DrawString(c.ToString(), font, brush, x, y);
                x += cw;

                if (x + cw * 2 >= commandEditor.Info.TextWidth - cw && i != str.Length - 1)
                {
                    g.DrawString("…", font, brush, x, y);
                    return;
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
            else if (e.KeyData == Keys.Tab && window != null && window.Visible)
                InsertCompleteString();
            else if (e.KeyData == Keys.Up && window != null && window.Visible)
                window.SelectUp();
            else if (e.KeyData == Keys.Down && window != null && window.Visible)
                window.SelectDown();
        }

        private void InsertCompleteString()
        {
            var len = (lastLookupInput ?? "").Length;
            var str = window.SelectedItem.Substring(len);
            commandEditor.RunCommand("editor.insertrange", str.MakeCharacters());
            HideAutocompleteWindow();
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
            Editor.Redraw();
            var ed = GetEditor();
            ed.Top = (lastBounds.Height - ed.Height) / 2;
            ed.Left = Editor.ClientSize.Width / 20;
            ed.Width = PreferredEditorWidth;
            ed.Visible = true;
        }

        private void HideEditor()
        {
            var hidden = false;

            if (commandEditor != null && commandEditor.Visible)
            {
                ResetBuffer();
                commandEditor.Visible = false;
                hidden = true;
            }

            if (HideAutocompleteWindow() || hidden)
                Editor.Redraw();
        }

        private void ExecuteCommand(string command)
        {
            var stat = CommandParser.Parse(command);
            var cmd = ComponentCatalog.Instance.GetCommandByAlias(stat.Command);

            if (cmd != null)
                cmd.Run(Editor, stat.Argument);
        }

        internal int PreferredEditorWidth => Editor.ClientSize.Width - (Editor.ClientSize.Width / 10);

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}
