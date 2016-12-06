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
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Margins;
using CodeBox.Core.Keyboard;

namespace CodeBox.CommandLine
{
    public class CommandMargin : Margin
    {
        private Editor commandEditor;
        private AutocompleteWindow window;
        private Rectangle lastBounds;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisPath
        };

        public CommandMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            var ed = GetEditor();

            if (ed.Width != PreferredEditorWidth)
                HideEditor();

            var cs = (CommandBarStyle)Editor.Theme.GetStyle(StandardStyle.CommandBar);
            g.FillRectangle(cs.BackColor.Brush(), bounds);
            lastBounds = bounds;

            if (!ed.Visible)
            {
                var font = Editor.Settings.SmallFont.Get(cs.FontStyle);//SysFont.Font;// 
                var x = Editor.Info.CharWidth * 2f;
                var h = Editor.Info.CharWidth;
                var y = bounds.Y + ((bounds.Height - h) / 2);
                var w = Editor.Info.CharWidth;

                if (Editor.Buffer.IsDirty)
                    g.FillRectangle(cs.ForeColor.Brush(), x, y, w, h);
                else
                    g.DrawRectangle(cs.ForeColor.Pen(), x, y, w, h);

                y = (bounds.Height - font.Height) / 2;
                x = Editor.Info.CharWidth * 4f;
                g.DrawString(Editor.Buffer.File.Name, font, cs.ForeColor.Brush(), x, y, TextStyle.Format);
                x += /*g.MeasureString(Editor.Buffer.File.Name, font).Width;*/(Editor.Buffer.File.Name.Length + 1.5f) * Editor.Info.SmallCharWidth;

                g.DrawString(Editor.Buffer.File.DirectoryName, font, cs.SpecialColor.Brush(), 
                    new RectangleF(x, y, bounds.Width - x - Editor.Info.CharWidth, bounds.Height), format);
            }
            else
            {
                var shift = Dpi.GetHeight(2);
                g.FillRectangle(ed.BackColor.Brush(),
                    new Rectangle(ed.Left - Editor.Info.SmallCharWidth, shift,
                    ed.Width + Editor.Info.SmallCharWidth * 2, bounds.Height - shift * 2));
            }

            return true;
        }

        public override MarginEffects MouseDown(Point loc)
        {
            //ShowEditor();
            ComponentCatalog.Instance.RunCommand(Editor, (Identifier)"file.openfile");
            return MarginEffects.CaptureMouse;
        }

        public override MarginEffects MouseUp(Point loc)
        {
            GetEditor().Focus();
            commandEditor.Redraw();
            return MarginEffects.None;
        }

        private Editor GetEditor()
        {
            if (commandEditor == null)
            {
                commandEditor = new LineEditor(Editor);
                commandEditor.Visible = false;
                commandEditor.LostFocus += EditorLostFocus;
                commandEditor.KeyDown += EditorKeyDown;
                commandEditor.Paint += EditorPaint;
                commandEditor.Styles.StyleNeeded += EditorStyleNeeded;
                commandEditor.CommandRejected += EditorCommandRejected;
                ResetBuffer();
                Editor.Controls.Add(commandEditor);
                commandEditor.BringToFront();
            }

            return commandEditor;
        }

        private void EditorCommandRejected(object sender, EventArgs e)
        {
            var key = KeyboardAdapter.Instance.LastKey;

            if (key.Name != "newline" && key.Name != "up" && key.Name != "down" && key.Name != "indent")
                Editor.RunCommand(KeyboardAdapter.Instance.LastKey);
        }

        private AutocompleteWindow GetAutocompleteWindow()
        {
            if (window == null)
            {
                window = new AutocompleteWindow(Editor) { MaxItems = 15, SmallFont = true };
                window.Visible = false;
                window.MouseDown += AutocompleteClick;
                Editor.FindForm().Controls.Add(window);
                window.BringToFront();
            }

            return window;
        }

        private string lastLookupInput;
        private ArgumentAffinity currentAffinity;
        private void ShowAutocompleteWindow(IArgumentValueProvider prov)
        {
            var wnd = GetAutocompleteWindow();
            wnd.PreferredWidth = commandEditor.Width + Editor.Info.SmallCharWidth*2;
            var items = prov.EnumerateArgumentValues(lastLookupInput);

            if (items.Any()
                && (!items.JustOne()
                    || !items.First().Value.Equals(lastLookupInput, StringComparison.OrdinalIgnoreCase)))
            {
                wnd.SetItems(items);
                wnd.Left = commandEditor.Left - Editor.Info.SmallCharWidth;
                wnd.Top = lastBounds.Top + lastBounds.Height;
                window.Trimming = currentAffinity == ArgumentAffinity.FilePath ?
                    StringTrimming.EllipsisPath : StringTrimming.EllipsisCharacter;
                wnd.Visible = true;
                wnd.Invalidate();
                Editor.LockMouseScrolling = true;
            }
            else
                HideAutocompleteWindow();
        }

        private void AutocompleteClick(object sender, MouseEventArgs e)
        {
            if (window != null && window.Visible)
            {
                var arg = GetCurrentArgument();
                if (arg != -1 && arg < statement.Arguments.Count)
                {
                    var a = statement.Arguments[arg];
                    commandEditor.Buffer.Selections.Set(new Selection(
                        new Pos(0, a.Location.Start), new Pos(0, a.Location.End)));
                    commandEditor.RunCommand((Identifier)"editor.insertrange", window.SelectedItem.Value.MakeCharacters());
                }
                else
                    InsertCompleteString();
            }

            if (currentAffinity != ArgumentAffinity.FilePath
                && statement.Arguments.Count >= CommandCatalog.Instance.GetCommandByAlias(statement.Command)
                    ?.Arguments.Count(a => !a.Optional))
                ExecuteCommand(commandEditor.Text);
            else if (currentAffinity != ArgumentAffinity.FilePath)
                commandEditor.RunCommand((Identifier)"editor.insertrange", " ".MakeCharacters());
        }

        private bool HideAutocompleteWindow()
        {
            if (window != null && window.Visible)
            {
                window.Reset();
                window.Visible = false;
                Editor.LockMouseScrolling = false;
                return true;
            }

            return false;
        }

        private Statement statement;
        private void EditorStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            statement = new CommandParser(ContinuationStatement?.Clone()).Parse(commandEditor.Text);
            commandEditor.Styles.ClearStyles(0);

            if (statement == null)
                return;

            if (statement.Location.End != statement.Location.Start)
                commandEditor.Styles.StyleRange(StandardStyle.Keyword, 0,
                    statement.Location.Start, statement.Location.End);

            if (statement.HasArguments)
            {
                foreach (var a in statement.Arguments)
                {
                    commandEditor.Styles.StyleRange(StandardStyle.KeywordSpecial, 0,
                        a.Location.Start, a.Location.End);
                }
            }
        }

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statement != null && !string.IsNullOrWhiteSpace(statement.Command))
            {
                var spaced = commandEditor.Lines[0].Text.EndsWith(" ");
                var cmd = statement.Command;
                var arr =
                    CommandCatalog.Instance.EnumerateCommands()
                    .Where(c => c.Alias != null && (spaced ? c.Alias == cmd : c.Alias.StartsWith(cmd)))
                    .ToList();
                var g = e.Graphics;

                if ((arr.Count > 1 || (arr.Count == 1 && statement.Command.Length < arr[0].Alias.Length)) &&
                    !statement.HasArguments)
                {
                    DrawStringWithPeriods(g, string.Join(" ", arr.Select(a => a.Alias)), 0, hl: false);
                    HideAutocompleteWindow();
                }
                else if (arr.Count > 0)
                {
                    var ci = arr[0];
                    var ind = GetCurrentArgument(statement);
                    DrawStringWithPeriods(g, ci.ToString(), ind, hl: true);

                    if (ci.HasArguments)
                    {
                        var pos = commandEditor.Buffer.Selections.Main.Caret.Col;

                        if (ind != -1 && ind < ci.Arguments.Count && ci.Arguments[ind].ValueProvider != null)
                        {
                            var prov = ComponentCatalog.Instance.GetComponent(ci.Arguments[ind].ValueProvider)
                                as IArgumentValueProvider;
                            currentAffinity = ci.Arguments[ind].Affinity;

                            if (prov != null)
                            {
                                lastLookupInput = statement.HasArguments && statement.Arguments.Count > ind 
                                    ? statement.Arguments[ind].Value.ToString() : "";
                                ShowAutocompleteWindow(prov);
                                return;
                            }
                        }
                    }
                }
            }

            HideAutocompleteWindow();
        }

        private int GetCurrentArgument(Statement stmt = null)
        {
            stmt = stmt ?? statement;
            var pos = commandEditor.Buffer.Selections.Main.Caret.Col;

            if (!stmt.HasArguments && (pos > stmt.Location.End || stmt.Location.End == 0))
                return 0;

            if (stmt != null)
            {
                for (var i = 0; i < stmt.Arguments.Count; i++)
                {
                    var ar = stmt.Arguments[i];
                    if (ar.Location.Start <= pos && ar.Location.End >= pos)
                        return i;
                }
            }

            if (stmt.HasArguments && pos < stmt.Arguments[0].Location.Start)
                return 0;
            else if (stmt.HasArguments
                && (pos == 0 || pos > stmt.Arguments[stmt.Arguments.Count - 1].Location.End))
                return stmt.Arguments.Count;

            return -1;
        }

        private string TrimToSize(string str, int x)
        {
            var max = ((commandEditor.Info.TextWidth - x) / Editor.Info.SmallCharWidth) - 3;

            if (str.Length > max)
                str = str.Substring(0, max) + "…";

            return str;
        }

        private void DrawStringWithPeriods(Graphics g, string str, int arg, bool hl)
        {
            var tetras = commandEditor.Lines[0].GetTetras(commandEditor.IndentSize);
            var cw = commandEditor.Info.SmallCharWidth;
            var font = commandEditor.Settings.SmallFont;
            var x = (tetras + 2) * commandEditor.Info.CharWidth;
            var y = (commandEditor.Height - (int)(commandEditor.Info.SmallCharHeight * 1.1)) / 2;
            var brush = commandEditor.Theme.GetStyle(StandardStyle.Default).ForeColor.Brush();
            var brush1 = commandEditor.Theme.GetStyle(StandardStyle.KeywordSpecial).ForeColor.Brush();
            var brush2 = commandEditor.Theme.GetStyle(StandardStyle.Comment).ForeColor.Brush();
            var brush3 = commandEditor.Theme.GetStyle(StandardStyle.Keyword).ForeColor.Brush();
            var curarg = -1;
            var last = '\0';

            if (x + cw * 2 >= commandEditor.Info.TextWidth - cw)
                return;

            str = TrimToSize(str, x);
            var style = Editor.Theme.GetStyle(StandardStyle.Popup);
            g.DrawRoundedRectangle(style.BackColor, new Rectangle(x - Editor.Info.CharWidth, y,
                Editor.Info.SmallCharWidth * (str.Length + 2), (int)(commandEditor.Info.SmallCharHeight * 1.1)));
            var cmt = false;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if ((last == ' ' && curarg == -1) || last == '|')
                    curarg++;

                if (c == '#')
                    cmt = true;

                g.DrawString(c.ToString(), font, 
                    hl && cmt ? brush2
                    : hl && curarg == arg && arg != -1 && c != '|' ? brush1
                    : hl && curarg == -1 ? brush3 : brush, x, y, TextStyle.Format);
                x += cw;
                last = c;
            }
        }

        private void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                if (window != null && window.Visible)
                    AutocompleteClick(window, null);
                else
                {
                    var cl = commandEditor.Text;
                    HideEditor();
                    ExecuteCommand(cl);
                }
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
            var idx = GetCurrentArgument(statement);
            var sels = commandEditor.Buffer.Selections;

            if (!string.IsNullOrEmpty(lastLookupInput) && idx < statement.Arguments.Count)
            {
                sels.Main.Start = new Pos(0, statement.Arguments[idx].Location.Start);
                sels.Main.End = new Pos(0, statement.Arguments[idx].Location.End);
            }

            var str = window.SelectedItem.Value;
            commandEditor.RunCommand((Identifier)"editor.insertrange", str.MakeCharacters());
            HideAutocompleteWindow();
        }

        private void ResetBuffer(string text = "")
        {
            commandEditor.Buffer.Truncate(text);
            commandEditor.Buffer.CurrentLineIndicator = false;
            commandEditor.Buffer.ShowEol = false;
            commandEditor.Buffer.ShowLineLength = false;
            commandEditor.Buffer.ShowWhitespace = false;
            commandEditor.Buffer.WordWrap = false;
        }

        private void EditorLostFocus(object sender, EventArgs e) => Close();

        public void Show(Statement stmt)
        {
            ContinuationStatement = statement = stmt;
            Show(default(string));
            commandEditor.Redraw();
        }

        public void Show() => Show(default(string));

        public void Show(string cmd)
        {
            ShowEditor();

            if (cmd != null)
            {
                ResetBuffer(cmd + " ");
                commandEditor.Buffer.Selections.Set(new Pos(0, cmd.Length + 1));
            }

            commandEditor.Focus();
        }

        public void Close() => HideEditor();

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

        private static readonly object[] defargs = new object[0];
        private void ExecuteCommand(string command)
        {
            statement = new CommandParser(ContinuationStatement?.Clone()).Parse(command);
            var md = statement != null && statement.Command != null ? CommandCatalog.Instance.GetCommandByAlias(statement.Command) : null;

            if (md != null)
            {
                var args = statement.HasArguments
                    ? statement.Arguments.Select(a => a.Value).ToArray() : defargs;
                statement = null;
                ContinuationStatement = null;
                HideEditor();
                var exec = ComponentCatalog.Instance.GetComponent(md.Key.Namespace) as ICommandDispatcher;

                if (exec != null)
                    exec.Execute(Editor, md.Key, args);
            }
        }

        internal int PreferredEditorWidth => Editor.ClientSize.Width - (Editor.ClientSize.Width / 10);

        internal Statement ContinuationStatement { get; set; }

        internal bool IsActive => commandEditor != null && commandEditor.Visible;

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}
