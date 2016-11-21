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

        private void HideAutocompleteWindow()
        {
            if (window != null && window.Visible)
            {
                window.Visible = false;
                Editor.LockMouseScrolling = false;
                Editor.Redraw();
            }
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
                    else if (arr.Count > 1 || stmt.Command.Length < arr[0].Alias.Length)
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

                        HideAutocompleteWindow();
                    }
                    else
                    {
                        var a = arr[0];
                        var len = cw * (a.Key.Length + 1);

                        if (w + len < commandEditor.Info.TextWidth)
                            g.DrawString(a.Key, commandEditor.Font, st.ForeColor.Brush(), w, 0);

                        if (a.ArgumentType != ArgumentType.None)
                        {
                            w += len;
                            var str = $"({a.ArgumentName}:{a.ArgumentType.ToString().ToLower()})";

                            if (w + (str.Length + 1) * cw < commandEditor.Info.TextWidth)
                                g.DrawString(str, commandEditor.Font, st.ForeColor.Brush(), w, 0);
                        }

                        var obj = ComponentCatalog.Instance.GetCommandByAlias(a.Alias);
                        var prov = obj as IArgumentValueProvider;

                        if (prov != null)
                        {
                            var str = stmt.Argument != null ? stmt.Argument.ToString() : "";
                            lastLookupInput = str;
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
            new InsertRangeCommand(str.MakeCharacters()).Run(commandEditor);
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
            var ed = GetEditor();
            ed.Top = (lastBounds.Height - ed.Height) / 2;
            ed.Left = Editor.ClientSize.Width / 20;
            ed.Width = PreferredEditorWidth;
            ed.Visible = true;
        }

        private void HideEditor()
        {
            if (commandEditor != null && commandEditor.Visible)
            {
                ResetBuffer();
                commandEditor.Visible = false;
            }

            HideAutocompleteWindow();
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
