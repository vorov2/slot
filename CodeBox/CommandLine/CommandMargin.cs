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

namespace CodeBox.CommandLine
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
            var items = prov.EnumerateArgumentValues(lastLookupInput);

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

        private IEnumerable<Statement> statements;
        private void EditorStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            statements = CommandParser.Parse(commandEditor.Text);
            commandEditor.Styles.ClearStyles(0);

            if (statements == null)
                return;

            foreach (var stmt in statements)
            {
                commandEditor.Styles.StyleRange(StandardStyle.Keyword, 0,
                    stmt.Location.Start, stmt.Location.End);

                if (stmt.HasArguments)
                {
                    foreach (var a in stmt.Arguments)
                    {
                        var st = a.Type == ArgumentType.String ? StandardStyle.String
                            : a.Type == ArgumentType.Number ? StandardStyle.Number
                            : StandardStyle.Default;
                        commandEditor.Styles.StyleRange(st, 0,
                            a.Location.Start, a.Location.End);
                    }
                }
            }
        }

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statements != null && statements.Any())
            {
                var stmt = GetCurrentStatement();

                if (!string.IsNullOrWhiteSpace(stmt.Command))
                {
                    var cmd = stmt.Command;
                    var arr =
                        CommandCatalog.Instance.EnumerateCommands()
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
                        var ci = arr[0];
                        var sb = new StringBuilder();
                        sb.Append(ci.Key.Name);

                        if (ci.HasArguments)
                        {
                            for (var i = 0; i < ci.Arguments.Count; i++)
                            {
                                var ar = ci.Arguments[i];

                                if (i == 0)
                                    sb.Append('(');
                                else
                                    sb.Append(',');

                                sb.Append(ar.Name);
                                sb.Append(':');
                                sb.Append(ar.Type.ToString().ToLower());

                                if (i == ci.Arguments.Count - 1)
                                    sb.Append(')');
                            }
                        }
                        else
                            sb.Append("()");

                        if (ci.Title != null)
                            sb.Append(" //" + ci.Title);

                        DrawStringWithPeriods(g, sb.ToString());

                        if (ci.HasArguments)
                        {
                            var ind = GetCurrentArgument(stmt);
                            var pos = commandEditor.Buffer.Selections.Main.Caret.Col;

                            if (ind != -1 && ind < ci.Arguments.Count && ci.Arguments[ind].ValueProvider != null)
                            {
                                var prov = ComponentCatalog.Instance.GetComponent(ci.Arguments[ind].ValueProvider)
                                    as IArgumentValueProvider;

                                if (prov != null)
                                {
                                    lastLookupInput = stmt.HasArguments && stmt.Arguments.Count > ind 
                                        ? stmt.Arguments[ind].Value.ToString() : "";
                                    ShowAutocompleteWindow(prov);
                                    return;
                                }
                            }
                        }

                        HideAutocompleteWindow();
                    }
                }
            }
            else
                HideAutocompleteWindow();
        }

        private Statement GetCurrentStatement()
        {
            var pos = commandEditor.Buffer.Selections.Main.Caret.Col;
            foreach (var st in statements)
                if (st.Location.Start <= pos && st.Location.End >= pos)
                    return st;
            return statements.Last();
        }

        private int GetCurrentArgument(Statement stmt = null)
        {
            stmt = stmt ?? GetCurrentStatement();
            var pos = commandEditor.Buffer.Selections.Main.Caret.Col;

            if (!stmt.HasArguments && pos >= stmt.Location.End)
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
            else if (stmt.HasArguments && pos > stmt.Arguments[stmt.Arguments.Count - 1].Location.End
                && commandEditor.Lines[0][pos - 1].Char == ' ')
                return stmt.Arguments.Count;

            return -1;
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
            var stmt = GetCurrentStatement();
            var idx = GetCurrentArgument(stmt);
            var sels = commandEditor.Buffer.Selections;

            if (stmt.Arguments.Count < idx)
                sels.Set(new Pos(0, stmt.Arguments[idx].Location.End));

            var len = (lastLookupInput ?? "").Length;
            var instr = window.SelectedItem.Value.ToString();
            var hasnil = instr.IndexOf(' ') != -1 || instr.IndexOf('\t') != -1;
            var str = instr;

            if (stmt.Arguments.Count > idx && hasnil)
            {
                sels.Main.Start = new Pos(0, stmt.Arguments[idx].Location.Start);
                var end = stmt.Arguments[idx].Location.End;
                sels.Main.End = new Pos(0, end < commandEditor.Lines[0].Length ? end + 1 : end);
                str = "\"" + str + "\"";
            }
            else
            {
                str = instr.Substring(len);
                if (len > 0 && lastLookupInput[len - 1] != instr[len - 1])
                    str = char.IsLower(lastLookupInput[len - 1]) ? str.ToLower() : str.ToUpper();
                str = hasnil ? "\"" + str + "\"" : str;
                if (instr.Length == str.Length
                    && sels.Main.Caret.Col > 0
                    && commandEditor.Lines[0][sels.Main.Caret.Col - 1].Char != ' ')
                    str = " " + str;
            }

            commandEditor.RunCommand((Identifier)"editor.insertrange", str.MakeCharacters());

            if (hasnil)
                sels.Set(new Pos(0, sels.Main.Caret.Col - 1));

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
            //HideEditor();
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
            foreach (var stat in statements)
            {
                var md = CommandCatalog.Instance.GetCommandByAlias(stat.Command);

                if (md != null)
                {
                    var exec = ComponentCatalog.Instance.GetComponent(md.Key.Namespace) as ICommandDispatcher;
                    if (exec != null)
                        exec.Execute(Editor, md.Key);
                }
            }
        }

        internal int PreferredEditorWidth => Editor.ClientSize.Width - (Editor.ClientSize.Width / 10);

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}
