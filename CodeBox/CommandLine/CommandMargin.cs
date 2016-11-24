﻿using System;
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
                commandEditor.BeforePaste += EditorBeforePaste;
                commandEditor.Styles.StyleNeeded += EditorStyleNeeded;
                ResetBuffer();
                Editor.Controls.Add(commandEditor);
            }

            return commandEditor;
        }

        private void EditorBeforePaste(object sender, TextEventArgs e)
        {
            if (e.Text != null && (e.Text.IndexOf(' ') != -1 || e.Text.IndexOf('\t') != -1)
                && statement != null)
            {
                var idx = GetCurrentArgument();

                if (statement.Arguments.Count > idx)
                {
                    var arg = statement.Arguments[idx];

                    if (arg.Type != ArgumentType.String)
                    {
                        var oldpos = commandEditor.Buffer.Selections.Main.Caret.Col;
                        commandEditor.Buffer.Selections.Set(new Selection(
                            new Pos(0, arg.Location.Start),
                            new Pos(0, arg.Location.End)));
                        commandEditor.RunCommand((Identifier)"editor.insertrange", arg.Value.ToString().MakeCharacters());
                        commandEditor.Buffer.Selections.Set(new Pos(0, oldpos - 1));
                    }
                }
                else if (statement.Arguments.Count == idx)
                    e.Text = "\"" + e.Text + "\"";
            }
        }

        private AutocompleteWindow GetAutocompleteWindow()
        {
            if (window == null)
            {
                window = new AutocompleteWindow(Editor) { MaxItems = 15, SmallFont = true };
                window.Visible = false;
                window.MouseDown += AutocompleteClick;
                Editor.Controls.Add(window);
            }

            return window;
        }

        private string lastLookupInput;
        private ArgumentAffinity currentAffinity;
        private void ShowAutocompleteWindow(IArgumentValueProvider prov)
        {
            var wnd = GetAutocompleteWindow();
            wnd.PreferredWidth = commandEditor.Width;
            var items = prov.EnumerateArgumentValues(lastLookupInput);

            if (items.Any()
                && (!items.JustOne()
                    || !items.First().Data.ToString().Equals(lastLookupInput, StringComparison.OrdinalIgnoreCase)))
            {
                wnd.SetItems(items);
                wnd.Left = commandEditor.Left;
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
                InsertCompleteString();

            if (currentAffinity != ArgumentAffinity.FilePath
                && statement.Arguments.Count == CommandCatalog.Instance.GetCommandByAlias(statement.Command)
                    ?.Arguments.Count)
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
                    var st = a.Type == ArgumentType.String ? StandardStyle.String
                        : a.Type == ArgumentType.Number ? StandardStyle.Number
                        : StandardStyle.KeywordSpecial;
                    commandEditor.Styles.StyleRange(st, 0,
                        a.Location.Start, a.Location.End);
                }
            }
        }

        private void EditorPaint(object sender, PaintEventArgs e)
        {
            if (statement != null && !string.IsNullOrWhiteSpace(statement.Command))
            {
                var cmd = statement.Command;
                var arr =
                    CommandCatalog.Instance.EnumerateCommands()
                    .Where(c => c.Alias.StartsWith(cmd))
                    .ToList();
                var g = e.Graphics;

                if (arr.Count > 1 || (arr.Count == 1 && statement.Command.Length < arr[0].Alias.Length))
                {
                    DrawStringWithPeriods(g, string.Join(" ", arr.Select(a => a.Alias)), 0);
                    HideAutocompleteWindow();
                }
                else if (arr.Count == 1)
                {
                    var ci = arr[0];
                    var ind = GetCurrentArgument(statement);
                    DrawStringWithPeriods(g, ci.ToString(), ind);

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
                                Console.WriteLine($"Last lookup: {lastLookupInput}");
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
                && (pos == 0 || commandEditor.Lines[0].CharAt(pos - 1) == ' '
                    || pos > stmt.Arguments[stmt.Arguments.Count - 1].Location.End))
                return stmt.Arguments.Count;

            return -1;
        }

        private void DrawStringWithPeriods(Graphics g, string str, int arg)
        {
            var tetras = commandEditor.Lines[0].GetTetras(commandEditor.IndentSize);
            var cw = commandEditor.Info.SmallCharWidth;
            var font = commandEditor.Settings.SmallFont;
            var x = (tetras + 1) * commandEditor.Info.CharWidth;
            var y = (commandEditor.Height - (int)(commandEditor.Info.SmallCharHeight * 1.1)) / 2;
            var brush = commandEditor.Styles.Styles.GetStyle(StandardStyle.SpecialSymbol).ForeColor.Brush();
            var brush1 = commandEditor.Styles.Styles.DefaultStyle.ForeColor.Brush();
            var curarg = -1;
            var last = '\0';

            if (x + cw * 2 >= commandEditor.Info.TextWidth - cw)
                return;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if (last == '(' || last == ',')
                    curarg++;

                if (c == ')')
                    curarg = -1;

                g.DrawString(c.ToString(), font, curarg == arg && arg != -1 && c != ',' ? brush1 : brush, x, y);
                x += cw;

                if (x + cw * 2 >= commandEditor.Info.TextWidth - cw && i != str.Length - 1)
                {
                    g.DrawString("…", font, brush, x, y);
                    return;
                }

                last = c;
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
            var idx = GetCurrentArgument(statement);
            var sels = commandEditor.Buffer.Selections;

            if (statement.Arguments.Count > idx)
                sels.Set(new Pos(0, statement.Arguments[idx].Location.End));

            var len = (lastLookupInput ?? "").Length;
            var instr = window.SelectedItem.Data.ToString();
            var hasnil = instr.IndexOf(' ') != -1 || instr.IndexOf('\t') != -1;
            var str = instr;

            if (statement.Arguments.Count > idx && hasnil)
            {
                sels.Main.Start = new Pos(0, statement.Arguments[idx].Location.Start);
                var end = statement.Arguments[idx].Location.End;
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

        private void ResetBuffer(string text = "")
        {
            commandEditor.Text = text;
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

        public void Toggle(string cmd = null)
        {
            if (commandEditor == null || !commandEditor.Visible)
            {
                ShowEditor();

                if (cmd != null)
                {
                    ResetBuffer(cmd + " ");
                    commandEditor.Buffer.Selections.Set(new Pos(0, cmd.Length + 1));
                }

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

        private static readonly object[] defargs = new object[0];
        private void ExecuteCommand(string command)
        {
            statement = new CommandParser(ContinuationStatement?.Clone()).Parse(command);
            var md = CommandCatalog.Instance.GetCommandByAlias(statement.Command);

            if (md != null)
            {
                if (md.Arguments.Count(a => !a.Optional) > statement.Arguments.Count)
                {
                    statement.Location = default(Loc);
                    if (statement.HasArguments)
                        foreach (var a in statement.Arguments)
                            a.Location = new Loc(-1, -1);
                    ContinuationStatement = statement;
                    ShowEditor();
                    commandEditor.Focus();
                    commandEditor.Redraw();
                }
                else
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
        }

        internal int PreferredEditorWidth => Editor.ClientSize.Width - (Editor.ClientSize.Width / 10);

        internal Statement ContinuationStatement { get; set; }

        public override int CalculateSize() => Editor.Info.LineHeight + Editor.Info.CharWidth;
    }
}