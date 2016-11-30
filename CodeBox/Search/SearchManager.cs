using CodeBox.Commands;
using CodeBox.Drawing;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Search
{
    public sealed class SearchManager
    {
        private readonly Editor editor;
        private SearchWindow overlay;

        public SearchManager(Editor editor)
        {
            this.editor = editor;
        }

        private SearchWindow GetOverlay()
        {
            if (overlay == null)
            {
                overlay = new SearchWindow(editor);
                overlay.Visible = false;
                overlay.SearchBox.ContentModified += SearchBoxContentModified;
                overlay.SettingsChanged += SearchBoxContentModified;
                editor.Controls.Add(overlay);
            }

            return overlay;
        }


        private readonly List<SearchResult> finds = new List<SearchResult>();
        private void SearchBoxContentModified(object sender, EventArgs e)
        {
            Search();
            editor.Buffer.RequestRedraw();
        }

        private void Search()
        {
            var txt = overlay.SearchBox.Buffer.GetText();
            ClearFinds();

            if (!string.IsNullOrEmpty(txt))
            {
                var regex = TryGetRegex(txt);

                if (regex != null)
                {
                    for (var i = 0; i < editor.Lines.Count; i++)
                    {
                        var line = editor.Lines[i];
                        var ln = line.Text;

                        foreach (Match match in regex.Matches(ln))
                        {
                            var grp = match.Groups[match.Groups.Count - 1];
                            var aps = new AppliedStyle((int)StandardStyle.SearchItem, grp.Index, grp.Index + grp.Length - 1);
                            line.AppliedStyles.Add(aps);
                            finds.Add(new SearchResult(i, aps));
                        }
                    }
                }
            }
        }

        private Regex TryGetRegex(string txt)
        {
            try
            {
                overlay.InputInvalid = false;
                var opt = RegexOptions.None;

                if (!overlay.CaseSensitive)
                    opt |= RegexOptions.IgnoreCase;

                if (!overlay.UseRegex)
                    txt = Regex.Escape(txt);

                if (overlay.WholeWord)
                    txt = "\\b" + txt + "\\b";

                return new Regex(txt, opt);
            }
            catch
            {
                overlay.InputInvalid = true;
                return null;
            }
        }

        private void ClearFinds()
        {
            foreach (var f in finds)
            {
                if (editor.Lines.Count > f.Line)
                    editor.Lines[f.Line].AppliedStyles.Remove(f.Style);
            }

            finds.Clear();
        }

        public void TrySearch()
        {
            if (overlay != null && overlay.Visible)
                Search();
        }

        public void HideSearch()
        {
            if (overlay != null)
                overlay.Visible = false;
            ClearFinds();
        }

        public void UpdateSearchPanel()
        {
            if (overlay != null && overlay.Visible)
                InternalShowSearch(lastWidth, update: true);
        }

        private int lastWidth;
        public void ShowSearch()
        {
            InternalShowSearch(lastWidth = editor.Info.TextWidth / 2);
        }

        private void InternalShowSearch(int width, bool update = false)
        {
            var ovl = GetOverlay();

            if (!ovl.Visible || update)
            {
                if (width > editor.Info.TextWidth)
                    width = editor.Info.TextWidth;

                var size = new Size(width, editor.Info.LineHeight + Dpi.GetHeight(8));
                var rect = new Rectangle(new Point(
                        editor.Info.TextRight - size.Width - editor.Info.CharWidth,
                        editor.Info.TextTop + editor.Info.CharWidth),
                    size);
                ovl.Size = size;
                ovl.Location = rect.Location;
                ovl.Visible = true;
                ovl.Invalidate();
            }

            if (!editor.Buffer.Selections.Main.IsEmpty)
            {
                var txt = CopyCommand.GetTextRange(editor, editor.Buffer.Selections.Main);
                ovl.SearchBox.Text = txt;
                ovl.SearchBox.Buffer.Selections.Set(
                    new Selection(new Pos(0, 0), new Pos(0, txt.Length)));
            }

            if (!string.IsNullOrEmpty(ovl.SearchBox.Text))
                Search();

            ovl.SearchBox.Redraw();
            ovl.SearchBox.Focus();
        }

        public bool IsFocused => overlay != null && overlay.SearchBox.Focused;

        public bool IsSearchVisible => overlay != null && overlay.Visible;

        public bool HasSearchResults => finds.Count > 0;

        public IEnumerable<SearchResult> EnumerateSearchResults() => finds;

        public bool UseRegex
        {
            get { return GetOverlay().UseRegex; }
            set
            {
                GetOverlay().UseRegex = value;
                GetOverlay().Invalidate();
            }
        }

        public bool CaseSensitive
        {
            get { return GetOverlay().CaseSensitive; }
            set
            {
                GetOverlay().CaseSensitive = value;
                GetOverlay().Invalidate();
            }
        }

        public bool WholeWord
        {
            get { return GetOverlay().WholeWord; }
            set
            {
                GetOverlay().WholeWord = value;
                GetOverlay().Invalidate();
            }
        }
    }
}
