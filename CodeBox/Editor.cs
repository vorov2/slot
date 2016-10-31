using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.Commands;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using CodeBox.Drawing;
using CodeBox.Styling;
using CodeBox.Folding;

namespace CodeBox
{
    public class Editor : Control
    {
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERUPDATE = 0x0245;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_CHAR = 0x102;

        private sealed class EditorContext : IEditorContext
        {
            private readonly Editor editor;

            internal EditorContext(Editor editor)
            {
                this.editor = editor;
            }

            public ScrollingManager Scroll
            {
                get { return editor.Scroll; }
            }

            public CommandManager Commands
            {
                get { return editor.Commands; }
            }

            public DocumentBuffer Buffer
            {
                get { return editor.Buffer; }
            }

            public EditorInfo Info
            {
                get { return editor.Info; }
            }

            public EditorSettings Settings
            {
                get { return editor.Settings; }
            }

            public bool AtomicChange { get; set; }
        }

        private readonly IEditorContext context;

        public Editor()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw 
                | ControlStyles.Selectable, true);
            Cursor = Cursors.IBeam;
            TabStop = true;
            TopMargins = new MarginList(this);
            LeftMargins = new MarginList(this);
            RightMargins = new MarginList(this);
            BottomMargins = new MarginList(this);
            Info = new EditorInfo(this);
            Caret = new CaretRenderer(this);
            CachedBrush = new CachedBrush();
            Scroll = new ScrollingManager(this);
            Styles = new StyleManager(this);
            Settings = new EditorSettings(this);
            Commands = new CommandManager(this);
            Locations = new LocationManager(this);
            Folding = new FoldingManager(this);
            context = new EditorContext(this);
            Buffer = new DocumentBuffer(Document.Read(""));
            InitializeBuffer(Buffer);
        }

        private volatile bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Caret.Dispose();
                CachedBrush.Dispose();
                CachedFont.Dispose();
            }

            disposed = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POINTERUPDATE || m.Msg == WM_POINTERUP || m.Msg == WM_POINTERDOWN)
            {
                var x = m.LParam.ToInt32() & 0x0000FFFF;
                var y = (int)((m.LParam.ToInt32() & 0xFFFF0000) >> 16);
                var pos = PointToClient(new Point(x, y));

                if (m.Msg == WM_POINTERUPDATE)
                {
                    Scroll.OnPointerUpdate(pos);
                    return;
                }
                else if (m.Msg == WM_POINTERDOWN)
                {
                    Scroll.OnPointerDown(pos);
                    return;
                }
                else if (m.Msg == WM_POINTERUP)
                {
                    Scroll.OnPointerUp(pos);
                    return;
                }
            }
            else if (m.Msg == WM_MOUSEHWHEEL)
            {
                var delta = (short)((m.WParam.ToInt32() >> 16) & 0xFFFF);
                Scroll.ScrollX((delta / 120) * 2);
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);
        }

        private void InitializeBuffer(DocumentBuffer buffer)
        {
            buffer.Eol = Settings.Eol;
            buffer.WordWrap = Settings.WordWrap;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Caret.Suspend();

            var dt = DateTime.Now;

            e.Graphics.FillRectangle(Styles.Default.BackBrush,
                new Rectangle(Info.TextLeft, Info.TextTop, Info.TextWidth, Info.TextHeight));

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            DrawMargins(0, e.Graphics, TopMargins);
            DrawMargins(0, e.Graphics, LeftMargins);

            e.Graphics.TranslateTransform(Scroll.X, Scroll.Y);
            DrawLines(e.Graphics);
            //Console.WriteLine("Time: " + (DateTime.Now - dt).TotalMilliseconds);

            e.Graphics.ResetTransform();
            DrawMargins(Info.TextBottom, e.Graphics, BottomMargins);
            DrawMargins(ClientSize.Width - Info.CharWidth, e.Graphics, RightMargins);
            base.OnPaint(e);
        }

        private void DrawMargins(int start, Graphics g, MarginList margins)
        {
            var vertical = margins == LeftMargins || margins == RightMargins;
            var top = margins == TopMargins;

            foreach (var m in margins)
            {
                var bounds = default(Rectangle);

                if (vertical)
                    bounds = new Rectangle(start, Info.TextTop, m.CalculateSize(), ClientSize.Height - Info.TextTop);
                else if (top)
                    bounds = new Rectangle(0, start, ClientSize.Width, m.CalculateSize());
                else
                    bounds = new Rectangle(Info.TextLeft, start, Info.TextWidth, m.CalculateSize());

                m.Draw(g, bounds);
                start += m.CalculateSize();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            Scroll.InvalidateLines(force: true);
            Styles.Restyle();
            Invalidate();
            base.OnResize(eventargs);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Scroll.ScrollY((e.Delta / 120) * 2);
        }

        private void DrawLines(Graphics g)
        {
            var fvl = Scroll.FirstVisibleLine;
            var lvl = Scroll.LastVisibleLine;

            for (var i = fvl; i < lvl + 1; i++)
            {
                var ln = Document.Lines[i];
                if (!ln.Visible.Has(FoldingStates.Invisible))
                    DrawLine(g, ln, i);
            }
        }

        private void DrawLine(Graphics g, Line line, int lineIndex)
        {
            var lmarg = Info.TextLeft;
            var tmarg = Info.TextTop;
            var cwidth = Info.TextRight;
            var x = lmarg;
            var y = tmarg + line.Y;
            var oldcut = 0;
            var sel = Buffer.Selections.IsLineSelected(lineIndex);

            for (var j = 0; j < line.Stripes; j++)
            {
                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                for (var i = oldcut; i < cut; i++)
                {
                    var c = line.CharAt(i);
                    var xw = c == '\t' ? Settings.TabSize * Info.CharWidth : Info.CharWidth;
                    var visible = x + Scroll.X >= lmarg && x + Scroll.X + xw <= cwidth
                        && y + Scroll.Y >= tmarg;

                    if (visible)
                    {
                        var style = default(Style);
                        var rect = new Rectangle(x, y, xw, Info.LineHeight);
                        var pos = new Pos(lineIndex, i);
                        var high = sel && Buffer.Selections.IsSelected(pos);

                        if (c == '\0' || c == '\t' || c == ' ')
                        {
                            style = Styles.GetStyle((int)StandardStyle.SpecialSymbol);
                            style.NextStyle = null;
                        }
                        else
                            style = line.GetStyle(i, Styles);

                        if (high)
                        {
                            var sstyle = Styles.GetStyle((int)StandardStyle.Selection);
                            sstyle.NextStyle = style;
                            style = sstyle;
                        }

                        style.DrawAll(g, rect, pos);

                        if (Buffer.Selections.HasCaret(pos))
                        {
                            var blink = Buffer.Selections.Main.Caret.Line == lineIndex
                                && Buffer.Selections.Main.Caret.Col == i;

                            if (blink)
                            {
                                var cg = Caret.GetDrawingSurface();
                                cg.Clear(high ? Styles.Selection.Color : Styles.Default.BackColor);
                                style.DrawAll(cg, new Rectangle(default(Point), rect.Size), pos);
                                Caret.Resume();
                            }

                            Caret.DrawCaret(g, x, y, blink);
                        }
                    }

                    x += xw;
                }

                if (line.Visible.Has(FoldingStates.Header) && lineIndex < Lines.Count &&
                    Lines[lineIndex + 1].Visible.Has(FoldingStates.Invisible))
                {
                    Folding.DrawFoldingIndicator(g, x, y);
                }

                oldcut = cut;
                y += Info.LineHeight;
                x = lmarg;
            }
        }

        public override Color BackColor
        {
            get { return Styles.Default.BackColor; }
            set { Styles.Default.BackColor = value; }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            var pos = PointToClient(Cursor.Position);
            if (pos.X >= Info.TextLeft
                && pos.X <= Info.TextRight
                && pos.Y >= Info.TextTop
                && pos.Y <= Info.TextBottom)
                Commands.Run(MouseEvents.DoubleClick, Keys.None, default(CommandArgument));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseThief != null)
            {
                mouseThief.MouseMove(e.Location);
                return;
            }

            var mlist = Locations.FindMargin(e.Location);

            if (mlist != null)
            {
                mlist.CallMarginMethod(MarginMethod.MouseMove, e.Location);
                Cursor = Cursors.Arrow;
            }
            else
                Cursor = Cursors.IBeam;

            if (Mouse != MouseEvents.Click)
                return;

            var leftMouseDown = e.Button == MouseButtons.Left;
            var p = default(Pos);

            if (leftMouseDown)
            {
                p = Locations.LocationToPosition(e.Location);

                if (p.IsEmpty && e.Y - Scroll.Y > Info.TextIntegralHeight
                    && Scroll.LastVisibleLine < Lines.Count - 1)
                {
                    var lineIndex = Scroll.LastVisibleLine;
                    var ln = Document.Lines[lineIndex];
                    p = new Pos(lineIndex + 1, ln.Length <= p.Col ? p.Col : ln.Length);
                }
            }

            if (p.IsEmpty)
                return;

            var arg = new CommandArgument(p);
            Commands.Run(Mouse | MouseEvents.Move, LastKeys, arg);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Mouse = MouseEvents.None;
            LastKeys = Keys.None;

            if (mouseThief != null)
            {
                mouseThief.MouseUp(e.Location);
                mouseThief = null;
                Redraw();
            }

            var mlist = Locations.FindMargin(e.Location);

            if (mlist != null)
                mlist.CallMarginMethod(MarginMethod.MouseUp, e.Location);
        }
        
        internal Margin mouseThief;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                Mouse = MouseEvents.Click;
            else if (e.Button == MouseButtons.Right)
                Mouse = MouseEvents.RightClick;

            if (e.Button == MouseButtons.Left)
            {
                var mlist = Locations.FindMargin(e.Location);

                if (mlist != null)
                    mlist.CallMarginMethod(MarginMethod.MouseDown, e.Location);
                else
                {
                    var pos = Locations.LocationToPosition(e.Location);
                    pos = pos.IsEmpty ? default(Pos) : pos;
                    Commands.Run(Mouse, LastKeys, new CommandArgument(pos));
                    var idx = Buffer.Selections.IndexOfCaret(pos);
                    if (idx != -1)
                        Buffer.Selections[idx].SetToRestore();
                }
            }

            if (!Focused)
                Focus();
        }
        
        protected override bool ProcessMnemonic(char charCode)
        {
            return ProcessKey(charCode, LastKeys) || base.ProcessMnemonic(charCode);
        }

        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR)
                ProcessMnemonic(Convert.ToChar(m.WParam.ToInt32()));

            return base.ProcessKeyMessage(ref m);
        }

        private bool ProcessKey(char ch, Keys keys)
        {
            if (keys == Keys.None || keys == Keys.Shift)
            {
                switch (ch)
                {
                    case '\b':
                    case '\r':
                    case '\t':
                    case '\u001b': //Esc
                        return true;
                    default:
                        var arg = new CommandArgument(ch);
                        Commands.Run<InsertCharCommand>(arg);
                        break;
                }

                return true;
            }

            return false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            LastKeys = Keys.None;
        }

        internal void Redraw()
        {
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                case Keys.Tab:
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Insert:
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                case Keys.Shift | Keys.Tab:
                case Keys.Control | Keys.PageDown:
                case Keys.Control | Keys.PageUp:
                case Keys.Control | Keys.Home:
                case Keys.Control | Keys.End:
                case Keys.Control | Keys.Shift | Keys.End:
                case Keys.Control | Keys.Shift | Keys.Home:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Insert)
                Overtype = !Overtype;

            LastKeys = e.Modifiers;
            Commands.Run(LastKeys | e.KeyCode, default(CommandArgument));
        }
        
        public DocumentBuffer Buffer { get; private set; }

        public Document Document
        {
            get { return Buffer.Document; }
        }

        internal List<Line> Lines
        {
            get { return Document.Lines; }
        }

        public override string Text
        {
            get { return Buffer.GetText(); }
            set
            {
                base.Text = value;
                var doc = Document.Read(value);
                Buffer = new DocumentBuffer(doc);
                InitializeBuffer(Buffer);
                Scroll.InvalidateLines();
                Styles.Restyle();
                Folding.RebuildFolding();
            }
        }

        public bool Overtype
        {
            get { return Buffer.Overtype; }
            set
            {
                if (value != Buffer.Overtype)
                {
                    Buffer.Overtype = value;
                    Caret.BlockCaret = value;
                    Redraw();
                }
            }
        }

        internal bool AtomicChange
        {
            get { return context.AtomicChange; }
            set { context.AtomicChange = value; }
        }

        internal IEditorContext Context
        {
            get { return context; }
        }

        internal Keys LastKeys { get; private set; }

        internal MouseEvents Mouse { get; private set; }

        public EditorInfo Info { get; }

        public EditorSettings Settings { get; }

        public MarginList TopMargins { get; }

        public MarginList LeftMargins { get; }

        public MarginList RightMargins { get; }

        public MarginList BottomMargins { get; }

        internal CachedBrush CachedBrush { get; private set; }

        internal CachedFont CachedFont { get; set; }

        internal CaretRenderer Caret { get; }
        
        public CommandManager Commands { get; }

        public StyleManager Styles { get; }

        public ScrollingManager Scroll { get; }

        public LocationManager Locations { get; }

        public FoldingManager Folding { get; }
    }
}
