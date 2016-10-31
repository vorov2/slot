using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Styling;
using System.Drawing;
using CodeBox.ObjectModel;

namespace CodeBox
{
    public sealed class StyleManager
    {
        private readonly Dictionary<int, Style> styles = new Dictionary<int, Style>();
        private readonly Editor editor;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        public StyleManager(Editor editor)
        {
            this.editor = editor;
            Register(StandardStyle.Default, new TextStyle());
            Register(StandardStyle.Selection, new SelectionStyle());
            Register(StandardStyle.LineNumber, new TextStyle());
            Register(StandardStyle.CurrentLineNumber, new TextStyle());
            Register(StandardStyle.SpecialSymbol, new TextStyle());
            Register(StandardStyle.FoldingMarker, new MarkerStyle());
        }

        public Style GetStyle(int styleId)
        {
            return styles[styleId];
        }

        private void Register(StandardStyle styleId, Style style)
        {
            style.Editor = editor;
            styles.Add((int)styleId, style);

            switch (styleId)
            {
                case StandardStyle.Default:
                    Default = (TextStyle)style;
                    break;
                case StandardStyle.SpecialSymbol:
                    SpecialSymbol = (TextStyle)style;
                    break;
                case StandardStyle.LineNumber:
                    LineNumber = (TextStyle)style;
                    break;
                case StandardStyle.CurrentLineNumber:
                    CurrentLineNumber = (TextStyle)style;
                    break;
                case StandardStyle.Selection:
                    Selection = (SelectionStyle)style;
                    break;
                case StandardStyle.FoldingMarker:
                    FoldingMarker = (MarkerStyle)style;
                    break;
            }
        }

        public void Register(StyleId styleId, Style style)
        {
            style.Editor = editor;
            styles.Remove(styleId);
            styles.Add(styleId, style);
        }

        public void ClearStyles(int line)
        {
            editor.Lines[line].AppliedStyles.Clear();
        }

        public void StyleRange(int style, int line, int start, int end)
        {
            editor.Lines[line].AppliedStyles.Add(new AppliedStyle(style, start, end));
        }

        internal void Restyle()
        {
            if (editor.Lines.Count == 0)
                return;

            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;
            OnStyleNeeded(
                new Range(
                    new Pos(fvl, 0),
                    new Pos(lvl, editor.Lines[lvl].Length - 1)));
        }

        public event EventHandler<StyleNeededEventArgs> StyleNeeded;
        private void OnStyleNeeded(Range range)
        {
            StyleNeeded?.Invoke(this, new StyleNeededEventArgs(range));
        }

        #region Standard styles
        public TextStyle Default { get; private set; }

        public SelectionStyle Selection { get; private set; }

        public TextStyle SpecialSymbol { get; private set; }

        public MarkerStyle FoldingMarker { get; private set; }

        public TextStyle LineNumber { get; private set; }

        public TextStyle CurrentLineNumber { get; private set; }
        #endregion
    }
}
