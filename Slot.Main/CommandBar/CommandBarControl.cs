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

namespace Slot.Main.CommandBar
{
    public class CommandBarControl : Control
    {
        private EditorControl editor;
        private EditorControl commandEditor;
        private AutocompleteWindow window;
        private string error;
        private Rectangle errorButton;
        private MessageOverlay overlay;

        public CommandBarControl(EditorControl editor)
        {
            this.editor = editor;
            editor.Escape += EditorEscape;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Cursor = Cursors.Default;
            AdjustHeight();
            App.Catalog<ILogComponent>().Default().EntryWritten += (o, e) =>
            {
                var ovl = GetMessageOverlay();

                if (ovl != null && ovl.Visible)
                    HideTip();

                if (e.Type == EntryType.Error)
                    error = e.Data;
                else
                    error = null;

                Invalidate();
            };
        }

        private void EditorEscape(object sender, EventArgs e)
        {
            var ovl = GetMessageOverlay();

            if (ovl != null && ovl.Visible)
                HideTip();
        }

        private void AdjustHeight()
        {
            var h = (int)Math.Round(
                Math.Max(App.Catalog<ISettingsProvider>().Default().Get<EnvironmentSettings>().Font.Height() * 1.7,
                    editor.Info.CharHeight * 1.9), MidpointRounding.AwayFromZero);
            if (Height != h)
                Height = h;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustHeight();
            var ed = GetEditor();

            if (ed.Width != PreferredEditorWidth)
                HideEditor();

            var g = e.Graphics;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var bounds = ClientRectangle;
            var cs = editor.Theme.GetStyle(StandardStyle.CommandBar);
            var acs = editor.Theme.GetStyle(StandardStyle.CommandBarCaption);
            g.FillRectangle(cs.BackColor.Brush(), bounds);

            if (!ed.Visible)
            {
                var baseFont = App.Catalog<ISettingsProvider>().Default().Get<EnvironmentSettings>().Font;
                var font = baseFont.Get(cs.FontStyle);
                var x = font.Width() * 2f;
                var h = font.Width() / 1.8f;
                var y = bounds.Y + ((bounds.Height - h) / 2);
                var w = h;

                if (editor.Buffer.IsDirty)
                    g.FillEllipse(cs.ForeColor.Brush(), x, y, w, h);//g.FillRectangle(cs.ForeColor.Brush(), x, y, w, h);
                else
                {
                    g.DrawEllipse(cs.ForeColor.Pen(), x, y, w, h);
                }
                    //g.DrawRectangle(cs.ForeColor.Pen(), x, y, w, h);

                y = (bounds.Height - font.Height()) / 2;
                x += font.Width();
                g.DrawString(editor.Buffer.File.Name, font, cs.ForeColor.Brush(), x, y, TextFormats.Compact);
                x += g.MeasureString(editor.Buffer.File.Name, font).Width;

                var ws = App.Catalog<IViewManager>().Default().GetActiveView().Workspace.FullName.Length;
                var dirName = editor.Buffer.File.DirectoryName.Substring(ws).TrimStart('/', '\\');
                g.DrawString(dirName, font.Get(acs.FontStyle), acs.ForeColor.Brush(), 
                    new RectangleF(x, y, bounds.Width - x - font.Width(), bounds.Height), TextFormats.Path);

                var tipRect = new Rectangle(bounds.Width - font.Width() * 2,
                    bounds.Y + ((bounds.Height - font.Width()) / 2), font.Width(), font.Width());

                if (error != null)
                {
                    errorButton = tipRect;
                    g.FillRectangle(editor.Theme.GetStyle(StandardStyle.Error).ForeColor.Brush(), errorButton);
                }
                else
                {
                    errorButton = default(Rectangle);
                    g.FillRectangle(editor.Theme.GetStyle(StandardStyle.Default).BackColor.Brush(), tipRect);
                    g.DrawRectangle(ControlPaint.Dark(cs.BackColor, .2f).Pen(), tipRect);
                }
            }
            else
            {
                var shift = Dpi.GetHeight(2);
                g.FillRectangle(ed.BackColor.Brush(),
                    new Rectangle(ed.Left - editor.Info.SmallCharWidth, shift,
                    ed.Width + editor.Info.SmallCharWidth * 2, bounds.Height - shift * 2));
            }
        }

        private MessageOverlay GetMessageOverlay()
        {
            if (overlay == null)
            {
                overlay = new MessageOverlay();
                overlay.Visible = false;
                FindForm().Controls.Add(overlay);
                overlay.BringToFront();
            }

            return overlay;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var loc = e.Location;

            if (loc.X >= errorButton.Left && loc.X <= errorButton.Right
                && loc.Y >= errorButton.Top && loc.Y <= errorButton.Bottom)
                ToggleTip();
            else
                App.Ext.Run(editor, Cmd.OpenFile);

            Invalidate();
            base.OnMouseDown(e);
        }

        public void ToggleTip()
        {
            var wnd = GetMessageOverlay();
            if (wnd == null || !wnd.Visible)
                ShowTip();
            else
                HideTip();
        }

        private void ShowTip()
        {
            var font = App.Catalog<ISettingsProvider>().Default().Get<EnvironmentSettings>().Font;
            var eWidth = editor.Info.TextWidth / 2;
            Size size;
            var err = "M " + error;
            error = null;

            using (var g = CreateGraphics())
                size = g.MeasureString(err, font, eWidth).ToSize();

            var ovl = GetMessageOverlay();
            var xpad = Dpi.GetWidth(8);
            var ypad = Dpi.GetHeight(4);
            ovl.Padding = new Padding(xpad, ypad, xpad, ypad);
            ovl.Width = size.Width + xpad * 2 + ovl.BorderWidth * 2;
            ovl.Height = size.Height + ypad * 2 + ovl.BorderWidth * 2;
            ovl.Location = new Point(FindForm().ClientRectangle.Width - ovl.Width, ClientSize.Height + editor.Info.TextTop);
            ovl.Font = font;
            ovl.Text = err;
            ovl.Visible = true;
        }

        private void HideTip()
        {
            var ovl = GetMessageOverlay();
            if (ovl != null && ovl.Visible)
            {
                ovl.Visible = false;
                error = null;
            }
        }

        private EditorControl GetEditor()
        {
            if (commandEditor == null)
            {
                commandEditor = new LineEditor(editor);
                commandEditor.Visible = false;
                commandEditor.LostFocus += EditorLostFocus;
                commandEditor.KeyDown += EditorKeyDown;
                commandEditor.Paint += EditorPaint;
                commandEditor.Styles.StyleNeeded += EditorStyleNeeded;
                commandEditor.CommandRejected += EditorCommandRejected;
                ResetBuffer();
                FindForm().Controls.Add(commandEditor);
                commandEditor.BringToFront();
            }

            return commandEditor;
        }

        private void EditorCommandRejected(object sender, EventArgs e)
        {
            var key = KeyboardAdapter.Instance.LastKey;

            if (key.Name != "newline" && key.Name != "up" && key.Name != "down" && key.Name != "indent")
                App.Ext.Run(editor, KeyboardAdapter.Instance.LastKey);
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

        private string lastLookupInput;
        private ArgumentAffinity currentAffinity;
        private void ShowAutocompleteWindow(IArgumentValueProvider prov)
        {
            var wnd = GetAutocompleteWindow();
            wnd.PreferredWidth = commandEditor.Width + editor.Info.SmallCharWidth*2;
            var items = prov.EnumerateArgumentValues(lastLookupInput);

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
                    App.Ext.Run(commandEditor, (Identifier)"editor.insertrange", window.SelectedItem.Value.MakeCharacters());
                }
                else
                    InsertCompleteString();
            }

            if (currentAffinity != ArgumentAffinity.FilePath
                && statement.Arguments.Count >= App.Catalog<ICommandProvider>().Default().GetCommandByAlias(statement.Command)
                    ?.Arguments.Count(a => !a.Optional))
                ExecuteCommand(commandEditor.Text);
            else if (currentAffinity != ArgumentAffinity.FilePath)
                App.Ext.Run(commandEditor, (Identifier)"editor.insertrange", " ".MakeCharacters());
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

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statement != null && !string.IsNullOrWhiteSpace(statement.Command))
            {
                var spaced = commandEditor.Document.GetLine(0).Text.EndsWith(" ");
                var cmd = statement.Command;
                var arr =
                    App.Catalog<ICommandProvider>().Default().EnumerateCommands()
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
            var font = commandEditor.Settings.SmallFont;
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
            App.Ext.Run(commandEditor, (Identifier)"editor.insertrange", str.MakeCharacters());
            HideAutocompleteWindow();
        }

        private void ResetBuffer(string text = "")
        {
            commandEditor.Buffer.Truncate(text);
            commandEditor.Buffer.CurrentLineIndicator = false;
            commandEditor.Buffer.ShowEol = false;
            commandEditor.Buffer.ShowLineLength = false;
            commandEditor.Buffer.WordWrap = false;
        }

        private void EditorLostFocus(object sender, EventArgs e) => CloseInput();

        public void ShowInput(Statement stmt)
        {
            ContinuationStatement = statement = stmt;
            ShowInput(default(string));
            commandEditor.Invalidate();
        }

        public void ShowInput() => ShowInput(default(string));

        public void ShowInput(string cmd)
        {
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

        public void CloseInput() => HideEditor();

        private void ShowEditor()
        {
            HideTip();
            AdjustHeight();
            var ed = (LineEditor)GetEditor();
            ed.AdjustHeight();
            ed.Top = (ClientRectangle.Height - ed.Height) / 2;
            ed.Left = editor.ClientSize.Width / 20;
            ed.Width = PreferredEditorWidth;
            ed.Visible = true;
            Invalidate();
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
                Invalidate();
        }

        private static readonly object[] defargs = new object[0];
        private void ExecuteCommand(string command)
        {
            statement = new CommandParser(ContinuationStatement?.Clone()).Parse(command);
            var md = statement != null && statement.Command != null
                ? App.Catalog<ICommandProvider>().Default().GetCommandByAlias(statement.Command) : null;

            if (md != null)
            {
                var args = statement.HasArguments
                    ? statement.Arguments.Select(a => a.Value).ToArray() : defargs;
                statement = null;
                ContinuationStatement = null;
                HideEditor();
                App.Ext.Run(editor, md.Key, args);
            }
        }

        internal int PreferredEditorWidth => editor.ClientSize.Width - (editor.ClientSize.Width / 10);

        internal Statement ContinuationStatement { get; set; }

        internal bool IsActive => commandEditor != null && commandEditor.Visible;
    }
}
