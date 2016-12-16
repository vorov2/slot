using Slot.Editor.Autocomplete;
using Slot.Editor.Commands;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Editor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Editor.Autocomplete
{
    public sealed class AutocompleteManager
    {
        private readonly EditorControl editor;
        private AutocompleteWindow window;
        private StringBuilder completeString;
        private IEnumerable<string> items;
        private int lastCol;

        internal AutocompleteManager(EditorControl editor)
        {
            this.editor = editor;
        }

        private void InitializeWindow()
        {
            if (window == null)
            {
                window = new AutocompleteWindow(editor);
                window.Visible = false;
                window.MouseDown += WindowMouseDown;
                editor.Controls.Add(window);
            }

            window.SetScrollPositionY(0);
        }

        private void WindowMouseDown(object sender, MouseEventArgs e) => InsertCompleteString();

        private void InsertCompleteString()
        {
            var str = window.SelectedItem.Value.Substring(completeString.Length);
            editor.RunCommand((Identifier)"editor.insertrange", str.MakeCharacters());
            HideAutocomplete();
        }

        private DocumentCompleteSource completeSource = new DocumentCompleteSource();

        public void ShowAutocomplete(Pos pos)
        {
            completeSource.Initialize(editor);
            var list = completeSource.GetItems();
            ShowAutocomplete(pos, list);
        }

        public void ShowAutocomplete(Pos pos, IEnumerable<string> items)
        {
            this.items = items;
            InitializeWindow();
            FindCompleteString();
            var prefix = completeString.ToString();
            var newItems = items.Where(i => i != prefix && i.StartsWith(prefix));

            if (!newItems.Any())
            {
                HideAutocomplete();
                return;
            }

            window.SetItems(newItems.Select(i => new ValueItem(i)));
            SetLocationByPos(pos);
            WindowShown = true;
            lastCol = pos.Col;
            window.Visible = CheckVisible(window.Location);
        }

        private void FindCompleteString()
        {
            var caret = editor.Buffer.Selections.Main.Caret;
            var line = editor.Lines[caret.Line];
            var aff = editor.AffinityManager.GetAffinity(caret);
            var seps = aff.NonWordSymbols ?? editor.Settings.NonWordSymbols;
            var sb = new StringBuilder();

            if (caret.Col > 0)
                for (var i = caret.Col - 1; i > -1; i--)
                {
                    var c = line.CharAt(i);

                    if (c == ' ' || c == '\t' || seps.IndexOf(c) != -1)
                        break;
                    else
                        sb.Insert(0, c); ;
                }

            completeString = sb;
        }

        private void SetLocationByPos(Pos pos)
        {
            var pt = editor.Locations.PositionToLocation(pos);
            var y = pt.Y + window.Height > editor.Info.TextBottom
                ? pt.Y - editor.Info.LineHeight - window.Height : pt.Y;
            var x = pt.X + window.Width > editor.Info.TextRight
                ? pt.X - window.Width : pt.X;
            window.Location = new Point(x, y);
        }

        public void HideAutocomplete()
        {
            if (WindowShown)
            {
                WindowShown = false;
                window.Visible = false;
            }
        }

        private bool CheckVisible(Point loc)
        {
            return
                loc.X >= editor.Info.TextLeft
                && loc.X + window.Width <= editor.Info.TextRight
                && loc.Y >= editor.Info.TextTop
                && loc.Y + window.Height <= editor.Info.TextBottom;
        }

        internal bool InWindowLocation(Point loc)
        {
            var x = loc.X;
            var y = loc.Y;
            return x >= window.Location.X
                && y >= window.Location.Y
                && x <= window.Location.X + window.Width
                && y <= window.Location.Y + window.Height;
        }

        internal void ShiftLocation(int xShift, int yShift)
        {
            if (WindowShown)
            {
                var pt = new Point(window.Location.X - xShift, window.Location.Y - yShift);
                window.Visible = CheckVisible(pt);
                window.Location = pt;
            }
        }

        internal void UpdateAutocomplete()
        {
            InitializeWindow();
            var caret = editor.Buffer.Selections.Main.Caret;
            var line = editor.Lines[caret.Line];
            var c = line.CharAt(caret.Col > 0 ? caret.Col - 1 : caret.Col);

            if (caret.Col > lastCol)
                completeString.Append(line[caret.Col - 1]);
            else if (caret.Col < lastCol && completeString.Length > 0)
                completeString.Remove(completeString.Length - 1, 1);

            lastCol = caret.Col;
            
            if (c == '\t' || c == ' ')
                HideAutocomplete();
            else
            {
                var prefix = completeString.ToString();
                var newItems = items.Where(i => i.StartsWith(prefix));

                if (!newItems.Any())
                    HideAutocomplete();
                else
                {
                    window.SetItems(newItems.Select(i => new ValueItem(i)));
                    window.Invalidate();
                    SetLocationByPos(caret);
                }
            }
        }

        internal bool ListenKeys(Keys keys)
        {
            if (keys == Keys.Up)
            {
                window.SelectUp();
                return true;
            }
            else if (keys == Keys.Down)
            {
                window.SelectDown();
                return true;
            }
            else if (keys == Keys.Tab || keys == Keys.Return)
            {
                InsertCompleteString();
                return true;
            }

            return false;
        }

        public bool WindowShown { get; private set; }
    }
}
