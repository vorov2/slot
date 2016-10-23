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
        private readonly Dictionary<byte, StyleInfo> styles = new Dictionary<byte, StyleInfo>();
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
            styles.Add(0, Default = new StyleInfo(
                Color.Black,
                Color.Yellow,
                FontStyle.Regular
            ));
        }

        public void Register(StandardStyle style, StyleInfo info)
        {
            Register((byte)style, info);
        }

        public void Register(byte code, StyleInfo info)
        {
            if (code < 4)
                info = PopulateStyle(code, info);

            styles.Remove(code);
            styles.Add(code, info);
        }

        private StyleInfo PopulateStyle(byte code, StyleInfo info)
        {
            var old = default(StyleInfo);

            if (!styles.TryGetValue(code, out old))
                return info;

            info = new StyleInfo(
                info.ForeColor ?? old.ForeColor,
                info.BackColor ?? old.BackColor,
                info.FontStyle ?? old.FontStyle
            );

            if (code == (byte)StandardStyle.Default)
            {
                Default = info;
                editor.BackColor = info.BackColor.Value;
                editor.ForeColor = info.ForeColor.Value;
                editor.Font = editor.CachedFont.Create(info.FontStyle.Value);
            }

            return info;
        }

        public Font Font(StandardStyle style)
        {
            return Font((byte)style);
        }

        public Font Font(byte style)
        {
            var st = styles[style];
            var fs = default(FontStyle);

            if (st.FontStyle != null)
                fs = st.FontStyle.Value;
            else
                fs = Default.FontStyle.Value;

            return editor.CachedFont.Create(fs);
        }

        public Brush BackBrush(StandardStyle style)
        {
            return BackBrush((byte)style);
        }

        public Brush BackBrush(byte style)
        {
            var st = styles[style];
            var col = default(Color);

            if (st.BackColor != null)
                col = st.BackColor.Value;
            else
                col = Default.BackColor.Value;

            return editor.CachedBrush.Create(col);
        }

        public Brush ForeBrush(StandardStyle style)
        {
            return ForeBrush((byte)style);
        }

        public Brush ForeBrush(byte style)
        {
            var st = styles[style];
            var col = default(Color);

            if (st.ForeColor != null)
                col = st.ForeColor.Value;
            else
                col = Default.ForeColor.Value;

            return editor.CachedBrush.Create(col);
        }

        internal void Draw(Graphics g, char c, byte styleCode, RectangleF rec, bool selected)
        {
            var info = styles[styleCode];
            var font = info.FontStyle != null
                ? editor.CachedFont.Create(info.FontStyle.Value)
                : editor.CachedFont.Create(Default.FontStyle.Value);
            var fc = info.ForeColor != null
                ? editor.CachedBrush.Create(info.ForeColor.Value)
                : editor.CachedBrush.Create(Default.ForeColor.Value);

            if (!selected && info.BackColor != null && styleCode != 0)
                g.FillRectangle(editor.CachedBrush.Create(info.BackColor.Value), rec);
            else if (selected)
                g.FillRectangle(editor.CachedBrush.Create(editor.Settings.SelectionColor), rec);

            g.DrawString(c.ToString(), font, fc, rec.Location, format);
        }

        public void StyleRange(byte style, Range range)
        {
            var lines = editor.Lines;

            for (var i = range.Start.Line; i < range.End.Line + 1; i++)
            {
                var max = i == range.End.Line ? range.End.Col + 1 : lines[i].Length;

                for (var j = i == range.Start.Line ? range.Start.Col : 0; j < max; j++)
                {
                    var ln = lines[i];

                    if (ln.Length > j)
                        ln[j] = ln[j].WithStyle(style);
                }
            }
        }

        internal void Restyle()
        {
            var fvl = editor.Scroll.FirstVisibleLine;
            var lvl = editor.Scroll.LastVisibleLine;
            OnStyleNeeded(
                new Range(
                    new Pos(fvl, 0),
                    new Pos(lvl, editor.Lines[lvl].Length - 1)));
        }

        public StyleInfo Default { get; private set; }

        public event EventHandler<StyleNeededEventArgs> StyleNeeded;
        private void OnStyleNeeded(Range range)
        {
            StyleNeeded?.Invoke(this, new StyleNeededEventArgs(range));
        }
    }
}
