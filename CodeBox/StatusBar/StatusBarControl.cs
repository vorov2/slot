using CodeBox.Drawing;
using CodeBox.Margins;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using CodeBox.Lexing;
using CodeBox.Core.ComponentModel;
using CodeBox.Core;
using CodeBox.Core.CommandModel;

namespace CodeBox.StatusBar
{
    public sealed class StatusBarControl : Control
    {
        public StatusBarControl(Editor editor)
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Cursor = Cursors.Default;
            Editor = editor;
            Height = editor.Info.LineHeight + Dpi.GetHeight(2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = e.ClipRectangle;

            var backColor = Editor.Theme.GetStyle(StandardStyle.CommandBar).BackColor;//"#007ACC");
            var foreColor = Editor.Theme.DefaultStyle.ForeColor;
            g.FillRectangle(backColor.Brush(), bounds);

            var ys = Dpi.GetHeight(2);
            g.FillRectangle(ControlPaint.Dark(Editor.Theme.DefaultStyle.BackColor, .05f).Brush(),
                new Rectangle(bounds.X, bounds.Y, bounds.Width, ys));

            var rights = Tiles.Where(t => t.Alignment == TileAlignment.Right);
            var pad = Dpi.GetWidth(6);
            var x = bounds.X + bounds.Width - pad;

            foreach (var tile in rights)
            {
                tile.Font = Editor.Settings.SmallFont;
                var width = tile.MeasureWidth(g);
                x -= width;
                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys);

                if (tile.Hover)
                    g.FillRectangle(ControlPaint.Dark(backColor, .01f).Brush(), rect);

                tile.Left = x;
                tile.Right = x + width;
                tile.Draw(g, foreColor, rect);
            }

            var lefts = Tiles.Where(t => t.Alignment == TileAlignment.Left);
            x = bounds.X + pad;

            foreach (var tile in lefts)
            {
                tile.Font = Editor.Settings.SmallFont;
                var width = tile.MeasureWidth(g);
                var rect = new Rectangle(x, 0, width, bounds.Height);

                if (tile.Hover)
                    g.FillRectangle(ControlPaint.Light(backColor, 0.3f).Brush(), rect);

                tile.Draw(g, foreColor, new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys));
                tile.Left = x;
                tile.Right = x + width;
                x += width;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (tile == hover)
                return;

            if (hover != null)
                hover.Hover = false;

            if (tile != null)
                tile.Hover = true;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (hover != null)
            {
                hover.Hover = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);

            if (tile != null)
                tile.PerformClick();
        }

        public Editor Editor { get; }

        public List<StatusBarTile> Tiles { get; } = new List<StatusBarTile>();
    }

    public enum TileAlignment
    {
        Left,

        Right
    }

    public class StatusBarTile
    {
        public StatusBarTile(TileAlignment alignment)
        {
            Alignment = alignment;
        }

        internal static readonly StringFormat measureFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        internal static readonly StringFormat drawFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.None
        };

        public virtual void Draw(Graphics g, Color color, Rectangle rect)
        {
            g.DrawString(Text, Font, color.Brush(), rect, drawFormat);
        }

        public virtual int MeasureWidth(Graphics g)
        {
            var size = g.MeasureString(Text, Font, int.MaxValue, measureFormat);
            return (int)Math.Round(size.Width, MidpointRounding.AwayFromZero) + Dpi.GetWidth(10);
        }

        internal Font Font { get; set; }

        internal protected virtual void PerformClick() => OnClick();

        internal int Left { get; set; }

        internal int Right { get; set; }

        internal bool Hover { get; set; }

        public virtual string Text { get; set; }

        public TileAlignment Alignment { get; }

        public virtual bool Visible { get; set; }

        public event EventHandler Click;
        protected virtual void OnClick() => Click?.Invoke(this, EventArgs.Empty);
    }

    public sealed class ModeTile : StatusBarTile
    {
        private readonly Editor editor;

        public ModeTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.GrammarKey != null ?
                    ComponentCatalog.Instance.Grammars().GetGrammar(editor.Buffer.GrammarKey).Name : null;
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            var key = (Identifier)"test.changemode";
            var disp = ComponentCatalog.Instance.GetComponent(key.Namespace) as ICommandDispatcher;
            disp.Execute(editor, key);
            base.PerformClick();
        }
    }

    public sealed class LineEndingTile : StatusBarTile
    {
        private readonly Editor editor;

        public LineEndingTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.Eol.ToString().ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            base.PerformClick();
        }
    }

    public sealed class EncodingTile : StatusBarTile
    {
        private readonly Editor editor;

        public EncodingTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.Encoding.WebName.ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            base.PerformClick();
        }
    }
}
