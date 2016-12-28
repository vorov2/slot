using System;
using System.Drawing;
using System.Linq;
using Slot.Editor.ObjectModel;
using Slot.Editor.Styling;
using Slot.Drawing;
using System.Windows.Forms;
using Slot.Editor.Autocomplete;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.Keyboard;
using Slot.Core.Themes;
using Slot.Core.Settings;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.IO;
using Slot.Core.ViewModel;
using Slot.Editor;
using Slot.Core.Output;
using Slot.Editor.Drawing;
using System.Collections.Generic;

namespace Slot.Main.CommandBar
{
    public class CommandBarControl : Control
    {
        private EditorControl editor;
        private EditorControl commandEditor;
        private AutocompleteWindow window;

        public CommandBarControl(EditorControl editor)
        {
            this.editor = editor;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Cursor = Cursors.Default;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AdjustSize();
        }

        private void AdjustSize()
        {
            var frm = FindForm();
            var h = (int)Math.Round(
                Math.Max(((IView)frm).Settings.Get<EnvironmentSettings>().Font.Height() * 1.7,
                    editor.Info.CharHeight * 1.9), MidpointRounding.AwayFromZero)
                    + BorderHeight;
            if (Height != h)
                Height = h;
            if (Width != frm.ClientSize.Width)
                Width = frm.ClientSize.Width;
        }

        private int BorderHeight => (int)Math.Round(editor.Info.LineHeight * .1);

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustSize();
            var ed = GetEditor();

            if (ed.Width != PreferredEditorWidth || editor.Buffer == null)
            {
                Visible = false;
                return;
            }

            var g = e.Graphics;
            //e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var bounds = ClientRectangle;
            var cs = editor.Theme.GetStyle(StandardStyle.CommandBar);
            var acs = editor.Theme.GetStyle(StandardStyle.CommandBarCaption);
            g.FillRectangle(cs.BackColor.Brush(), bounds);
            var shift = Dpi.GetHeight(2);
            var rect = new Rectangle(ed.Left - editor.Info.SmallCharWidth, shift,
                ed.Width + editor.Info.SmallCharWidth * 2, bounds.Height - shift * 2);
            g.FillRectangle(ed.BackColor.Brush(), rect);

            var ds = editor.Theme.GetStyle(StandardStyle.Default);
            g.FillRectangle(ControlPaint.Dark(ds.BackColor, .05f).Brush(), 
                new RectangleF(bounds.X, bounds.Y + bounds.Height - BorderHeight, bounds.Width, BorderHeight));
        }

        private EditorControl GetEditor()
        {
            if (commandEditor == null)
            {
                commandEditor = new LineEditor(editor);
                commandEditor.LostFocus += EditorLostFocus;
                commandEditor.KeyDown += EditorKeyDown;
                commandEditor.Paint += EditorPaint;
                commandEditor.Styles.StyleNeeded += EditorStyleNeeded;
                commandEditor.CommandRejected += EditorCommandRejected;
                ResetBuffer();
                Controls.Add(commandEditor);
            }

            return commandEditor;
        }

        private void EditorCommandRejected(object sender, EventArgs e)
        {
            var km = App.Component<IKeyboardAdapter>();
            var key = km.LastKey;

            if (key.Name != "newline" && key.Name != "up" && key.Name != "down" && key.Name != "indent")
                App.Ext.Run(key);
        }

        private AutocompleteWindow GetAutocompleteWindow()
        {
            if (window == null)
            {
                window = new AutocompleteWindow(editor) { MaxItems = 15, SmallFont = true };
                window.Visible = false;
                window.MouseDown += AutocompleteClick;
                editor.FindForm().Controls.Add(window);
                window.BringToFront();
            }

            return window;
        }

        struct Sort
        {
            public int Diff;
            public ValueItem Val;
        }

        private string lastLookupInput;
        private ArgumentAffinity currentAffinity;
        private void ShowAutocompleteWindow(IArgumentValueProvider prov)
        {
            var wnd = GetAutocompleteWindow();
            wnd.PreferredWidth = commandEditor.Width + editor.Info.SmallCharWidth*2;
            var filtered = prov as IFilteredArgumentValueProvider;
            var items = filtered != null 
                ? filtered.EnumerateArgumentValues(lastLookupInput) : prov.EnumerateArgumentValues();

            if (filtered == null && !string.IsNullOrEmpty(lastLookupInput))
            {
                if (App.IsMono)
                {
                    var arr = lastLookupInput.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    items = items.Where(i => i.Value.ContainsAll(arr));
                }
                else
                {
                    var nitems = items.Select(i => new Sort { Diff = i.Value.LongestCommonSubstring(lastLookupInput), Val = i });
                    var max = nitems.Max(i => i.Diff);
                    items = nitems.Where(i => i.Diff == max).Select(i => i.Val);
                }
            }

            if (items.Any()
                && (!items.JustOne()
                    || !items.First().Value.Equals(lastLookupInput, StringComparison.OrdinalIgnoreCase)))
            {
                wnd.SetItems(items);
                wnd.Left = commandEditor.Left - editor.Info.SmallCharWidth;
                wnd.Top = Top + ClientRectangle.Height;
                window.Trimming = currentAffinity == ArgumentAffinity.FilePath ?
                    StringTrimming.EllipsisPath : StringTrimming.EllipsisCharacter;
                wnd.Visible = true;
                wnd.Invalidate();
                editor.LockMouseScrolling = true;
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
                    App.Ext.Run(commandEditor, Editor.Cmd.InsertRange, window.SelectedItem.Value.MakeCharacters());
                }
                else
                    InsertCompleteString();
            }

            if (currentAffinity != ArgumentAffinity.FilePath
                && statement.Arguments.Count >= App.Component<ICommandProvider>().GetCommandByAlias(statement.Command)
                    ?.Arguments.Count(a => !a.Optional))
                ExecuteCommand(commandEditor.Text);
            else if (currentAffinity != ArgumentAffinity.FilePath)
                App.Ext.Run(commandEditor, Editor.Cmd.InsertRange, " ".MakeCharacters());
        }

        private bool HideAutocompleteWindow()
        {
            if (window != null && window.Visible)
            {
                window.Reset();
                window.Visible = false;
                editor.LockMouseScrolling = false;
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

        private List<CommandMetadata> commandComplete;
        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statement != null && !string.IsNullOrWhiteSpace(statement.Command))
            {
                var spaced = commandEditor.Document.GetLine(0).Text.EndsWith(" ");
                var cmd = statement.Command;
                var arr = commandComplete =
                    App.Component<ICommandProvider>().EnumerateCommands()
                    .Where(c => c.Alias != null && (spaced ? c.Alias == cmd : c.Alias.StartsWith(cmd)))
                    .ToList();
                var g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.SmoothingMode = SmoothingMode.HighQuality;

                if ((arr.Count > 1 || (arr.Count == 1 && statement.Command.Length < arr[0].Alias.Length)) &&
                    !statement.HasArguments)
                {
                    DrawStringWithPeriods(g, string.Join(" ", arr.Select(a => a.Alias)), 0, hl: false);
                    HideAutocompleteWindow();
                }
                else if (arr.Count > 0)
                {
                    commandComplete = null;
                    var ci = arr[0];
                    var ind = GetCurrentArgument(statement);
                    DrawStringWithPeriods(g, ci.ToString(), ind, hl: true);

                    if (ci.HasArguments)
                    {
                        var pos = commandEditor.Buffer.Selections.Main.Caret.Col;

                        if (ind != -1 && ind < ci.Arguments.Count && ci.Arguments[ind].ValueProvider != null)
                        {
                            var prov = App.Catalog<IArgumentValueProvider>().GetComponent(ci.Arguments[ind].ValueProvider);
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
            else if (commandEditor.Text.Length == 0)
                DrawStringWithPeriods(e.Graphics, "#Enter a command alias", 0, true);

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
            var max = ((commandEditor.Info.TextWidth - x) / editor.Info.SmallCharWidth) - 3;

            if (str.Length > max)
                str = str.Substring(0, max) + "…";

            return str;
        }

        private void DrawStringWithPeriods(Graphics g, string str, int arg, bool hl)
        {
            var tetras = commandEditor.Document.GetLine(0).GetTetras(commandEditor.IndentSize);
            var cw = commandEditor.Info.SmallCharWidth;
            var font = commandEditor.EditorSettings.SmallFont;
            var x = (tetras + 2) * commandEditor.Info.CharWidth;
            var y = (commandEditor.Height - (int)(commandEditor.Info.SmallCharHeight * 1.1)) / 2;
            var style = editor.Theme.GetStyle(StandardStyle.Hint);
            var brush = style.ForeColor.Brush();
            var brush1 = commandEditor.Theme.GetStyle(StandardStyle.KeywordSpecial).ForeColor.Brush();
            var brush2 = commandEditor.Theme.GetStyle(StandardStyle.Comment).ForeColor.Brush();
            var brush3 = commandEditor.Theme.GetStyle(StandardStyle.Keyword).ForeColor.Brush();
            var curarg = -1;
            var last = '\0';

            if (x + cw * 2 >= commandEditor.Info.TextWidth - cw)
                return;

            str = TrimToSize(str, x);
            var width = editor.Info.SmallCharWidth * (str.Length + 2);
            var height = (int)(commandEditor.Info.SmallCharHeight * 1.1);
            g.DrawRoundedRectangle(style.BackColor, new Rectangle(x - editor.Info.CharWidth, y, width, height));
            var cmt = false;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if ((last == ' ' && curarg == -1) || last == '|')
                    curarg++;

                if (c == '#')
                    cmt = true;

                g.DrawString(c.ToString(), font.Get(style.FontStyle),
                    hl && cmt ? brush2
                    : hl && curarg == arg && arg != -1 && c != '|' ? brush1
                    : hl && curarg == -1 ? brush3 : brush, 
                    new Rectangle(x, y, width, editor.Info.SmallCharHeight), TextFormats.Centered);
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
                else if (commandComplete != null && commandComplete.Any())
                {
                    var cmd = commandComplete.First();
                    CloseInput();
                    ExecuteCommand(cmd.Alias);
                }
                else
                {
                    var cl = commandEditor.Text;
                    CloseInput();
                    ExecuteCommand(cl);
                }
            }
            else if (e.KeyData == Keys.Escape)
                CloseInput();
            else if (e.KeyData == Keys.Tab)
                InsertCompleteString();
            else if (e.KeyData == Keys.Up && window != null && window.Visible)
                window.SelectUp();
            else if (e.KeyData == Keys.Down && window != null && window.Visible)
                window.SelectDown();
        }

        private void InsertCompleteString()
        {
            string str;
            var sels = commandEditor.Buffer.Selections;

            if (commandComplete != null && commandComplete.Any())
            {
                var fst = commandComplete.First();
                str = fst.Alias + " ";

                if (statement != null)
                {
                    sels.Main.Start = new Pos(0, statement.Location.Start);
                    sels.Main.End = new Pos(0, statement.Location.End);
                }
            }
            else if (window != null && window.Visible)
            {
                var idx = GetCurrentArgument(statement);
                str = window.SelectedItem.Value;

                if (!string.IsNullOrEmpty(lastLookupInput) && idx < statement.Arguments.Count)
                {
                    sels.Main.Start = new Pos(0, statement.Arguments[idx].Location.Start);
                    sels.Main.End = new Pos(0, statement.Arguments[idx].Location.End);
                }
            }
            else
                return;

            App.Ext.Run(commandEditor, Editor.Cmd.InsertRange, str.MakeCharacters());
            HideAutocompleteWindow();
        }

        private void ResetBuffer(string text = "")
        {
            if (commandEditor.Buffer == null)
                commandEditor.Text = text;

            commandEditor.Buffer.Truncate(text);
            commandEditor.Buffer.CurrentLineIndicator = false;
            commandEditor.Buffer.ShowEol = false;
            commandEditor.Buffer.ShowLineLength = false;
            commandEditor.Buffer.WordWrap = false;
        }

        private void EditorLostFocus(object sender, EventArgs e) => CloseInput();

        public void OpenInput(Statement stmt)
        {
            ContinuationStatement = statement = stmt;
            OpenInput(default(string));
            commandEditor.Invalidate();
        }

        public void OpenInput() => OpenInput(default(string));

        public void OpenInput(string cmd)
        {
            Visible = true;
            ContinuationStatement = statement = null;
            ShowEditor();

            if (cmd != null)
            {
                ResetBuffer(cmd + " ");
                commandEditor.Buffer.Selections.Set(new Pos(0, cmd.Length + 1));
                commandEditor.Styles.RestyleDocument();
            }

            commandEditor.Focus();
        }

        private void ShowEditor()
        {
            AdjustSize();
            var ed = (LineEditor)GetEditor();
            var h = ClientRectangle.Height;// - BorderHeight;
            ed.AdjustHeight();
            ed.Top = (h - ed.Height) / 2;
            ed.Left = editor.ClientSize.Width / 20;
            ed.Width = PreferredEditorWidth;
            Invalidate();
        }

        public void CloseInput()
        {
            var hidden = false;

            if (commandEditor != null && commandEditor.Visible)
            {
                ResetBuffer();
                hidden = true;
            }

            if (HideAutocompleteWindow() || hidden)
                Invalidate();

            Visible = false;
        }

        private static readonly object[] defargs = new object[0];
        private void ExecuteCommand(string command)
        {
            statement = new CommandParser(ContinuationStatement?.Clone()).Parse(command);
            var md = statement != null && statement.Command != null
                ? App.Component<ICommandProvider>().GetCommandByAlias(statement.Command) : null;

            if (md != null)
            {
                var args = statement.HasArguments
                    ? statement.Arguments.Select(a => a.Value).ToArray() : defargs;
                statement = null;
                ContinuationStatement = null;
                CloseInput();
                App.Ext.Run(editor, md.Key, args);
            }
        }

        internal int PreferredEditorWidth => editor.ClientSize.Width - (editor.ClientSize.Width / 10);

        internal Statement ContinuationStatement { get; set; }

        internal bool IsActive => commandEditor != null && commandEditor.Visible;
    }
}
