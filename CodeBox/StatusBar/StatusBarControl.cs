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
            Height = editor.Info.LineHeight + Dpi.GetHeight(4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = e.ClipRectangle;
            var style = (MarginStyle)Editor.Theme.GetStyle(StandardStyle.StatusBar);
            g.FillRectangle(style.BackColor.Brush(), bounds);

            var ys = Dpi.GetHeight(2);
            g.FillRectangle(style.BackColor.Brush(),
                new Rectangle(bounds.X, bounds.Y, bounds.Width, ys));

            var rights = Tiles.Where(t => t.Alignment == TileAlignment.Right);
            var pad = Dpi.GetWidth(6);
            var x = bounds.X + bounds.Width - pad;
            var space = Editor.Info.SmallCharWidth;

            foreach (var tile in rights)
            {
                var foreColor = style.ForeColor;
                tile.Font = Editor.Settings.SmallFont;
                var width = tile.MeasureWidth(g);
                x -= width;
                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys*2);

                if (tile.Hover)
                {
                    g.FillRectangle(style.ActiveBackColor.Brush(), rect);
                    foreColor = style.ActiveForeColor;
                }

                tile.Left = x;
                tile.Right = x + width;
                tile.Draw(g, foreColor, rect);
                x -= space;
            }

            var lefts = Tiles.Where(t => t.Alignment == TileAlignment.Left);
            x = bounds.X + pad;

            foreach (var tile in lefts)
            {
                var foreColor = style.ForeColor;
                tile.Font = Editor.Settings.SmallFont;
                var width = tile.MeasureWidth(g);
                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys*2);

                if (tile.Hover)
                {
                    g.FillRectangle(style.ActiveBackColor.Brush(), rect);
                    foreColor = style.ActiveForeColor;
                }

                tile.Draw(g, foreColor, rect);
                tile.Left = x;
                tile.Right = x + width;
                x += width + space;
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



    public sealed class HelpTile : StatusBarTile
    {
        private readonly Editor editor;

        public HelpTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get { return "?"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            var key = (Identifier)"test.commandpalette";
            var disp = ComponentCatalog.Instance.GetComponent(key.Namespace) as ICommandDispatcher;
            disp.Execute(editor, key);
            base.PerformClick();
        }
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

    public sealed class PosTile : StatusBarTile
    {
        private readonly Editor editor;

        public PosTile(Editor editor) : base(TileAlignment.Left)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                var caret = editor.Buffer.Selections.Main.Caret;
                return $"Ln {caret.Line + 1}, Ch {caret.Col + 1}";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            base.PerformClick();
        }
    }
    public sealed class OvrTile : StatusBarTile
    {
        private readonly Editor editor;

        public OvrTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Overtype ? "OVR" : "INS";
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            editor.Overtype = !editor.Overtype;
            editor.Focus();
        }
    }
}
