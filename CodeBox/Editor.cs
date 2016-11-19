using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using CodeBox.Commands;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using CodeBox.Drawing;
using CodeBox.Styling;
using CodeBox.Folding;
using CodeBox.Indentation;
using CodeBox.Autocomplete;
using CodeBox.Affinity;
using CodeBox.CallTips;
using CodeBox.Core;

namespace CodeBox
{
    public class Editor : Control, IEditorContext
    {
        public readonly static bool Mono = Type.GetType("Mono.Runtime") != null;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERUPDATE = 0x0245;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_CHAR = 0x102;

        private readonly Timer timer = new Timer { Interval = 500 };
        private Pos movePosition;
        private Point mousePosition;

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

            CaretRenderer = new CaretRenderer(this);
            Renderer = new Renderer(this);
            CachedBrush = new CachedBrush();
            CachedPen = new CachedPen();
            Scroll = new ScrollingManager(this);
            Styles = new StyleManager(this);
            Commands = new CommandManager(this);
            Locations = new LocationManager(this);//+
            Folding = new FoldingManager(this) { Provider = new IndentFoldingProvider() };
            CallTips = new CallTipManager(this);
            MatchBrackets = new MatchBracketManager(this);
            Autocomplete = new AutocompleteManager(this);
            AffinityManager = new AffinityManager(this);
            Settings = new EditorSettings(this);

            Buffer = new DocumentBuffer(Document.Read(""));
            InitializeBuffer(Buffer);
            timer.Tick += Tick;
        }

        private volatile bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                CaretRenderer.Dispose();
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
            CaretRenderer.Suspend();

            var dt = DateTime.Now;

            e.Graphics.FillRectangle(Styles.Default.BackBrush,
                new Rectangle(Info.TextLeft, Info.TextTop, Info.TextWidth, Info.TextHeight));

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            DrawMargins(0, e.Graphics, TopMargins);
            DrawMargins(0, e.Graphics, LeftMargins);
            e.Graphics.TranslateTransform(Scroll.ScrollPosition.X, Scroll.ScrollPosition.Y);

            var carets = new List<CaretData>();
            DrawLines(e.Graphics, carets);
            
            e.Graphics.ResetTransform();
            DrawMargins(Info.TextBottom, e.Graphics, BottomMargins);
            if (RightMargins.Count > 0)
                DrawMargins(ClientSize.Width - RightMargins.First().CalculateSize(), e.Graphics, RightMargins);

            Renderer.DrawLongLineIndicators(e.Graphics);
            Renderer.DrawWordWrapColumn(e.Graphics);

            e.Graphics.TranslateTransform(Scroll.ScrollPosition.X, Scroll.ScrollPosition.Y);
            foreach (var c in carets)
                CaretRenderer.DrawCaret(e.Graphics, c.X, c.Y, c.Blink);

            Console.WriteLine("OnPaint time: " + (DateTime.Now - dt).TotalMilliseconds);
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
            Scroll.InvalidateLines(InvalidateFlags.Force);
            Styles.Restyle();
            Invalidate();
            Autocomplete.HideAutocomplete();
            base.OnResize(eventargs);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Autocomplete.WindowShown && Autocomplete.InWindowLocation(
                Mono ? PointToClient(e.Location) : e.Location))
                return;

            base.OnMouseWheel(e);
            Scroll.ScrollY((e.Delta / 120) * 2);
        }

        private void DrawLines(Graphics g, List<CaretData> carets)
        {
            var fvl = Scroll.FirstVisibleLine;
            var lvl = Scroll.LastVisibleLine;

            for (var i = fvl; i < lvl + 1; i++)
            {
                var ln = Document.Lines[i];
                if (!ln.Folding.Has(FoldingStates.Invisible))
                    DrawLine(g, ln, i, carets);
            }
        }

        private void DrawLine(Graphics g, Line line, int lineIndex, List<CaretData> carets)
        {
            var lmarg = Info.TextLeft;
            var tmarg = Info.TextTop;
            var cwidth = Info.TextRight;
            var x = lmarg;
            var y = tmarg + line.Y;
            var oldcut = 0;
            var sel = Buffer.Selections.IsLineSelected(lineIndex);
            var showEol = ShowEol;
            var showWs = ShowWhitespace;

            for (var j = 0; j < line.Stripes; j++)
            {
                var curline = CurrentLineIndicator 
                    && (Buffer.Selections.Main.IsEmpty || Buffer.Selections.Main.Start.Line == Buffer.Selections.Main.End.Line)
                    && lineIndex == Buffer.Selections.Main.Caret.Line;

                if (curline)
                {
                    g.FillRectangle(CachedBrush.Create(Settings.CurrentLineIndicatorColor),
                          new Rectangle(lmarg - Scroll.ScrollPosition.X, y, Info.TextWidth, Info.LineHeight));
                }

                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                var tet = 0;

                for (var i = oldcut; i < cut; i++)
                {
                    var c = line.CharAt(i);
                    var ct = c == '\t' ? Line.GetIndentationSize(tet, Context.IndentSize) : 1;
                    tet += ct;
                    var xw = ct * Info.CharWidth;
                    var visible = x + Scroll.ScrollPosition.X >= lmarg && x + Scroll.ScrollPosition.X + xw <= cwidth
                        && y + Scroll.ScrollPosition.Y >= tmarg;

                    if (visible)
                    {
                        var style = default(Style);
                        var rect = new Rectangle(x, y, xw, Info.LineHeight);
                        var pos = new Pos(lineIndex, i);
                        var high = sel && Buffer.Selections.IsSelected(pos);

                        if (c == '\0' && showEol || (c == '\t' || c == ' ') && showWs)
                        {
                            c = c == '\0' ? '\u00B6' : c == '\t' ? '\u2192' : '·';
                            style = /*line.GetStyle(i, Styles)
                                .Combine(*/Styles.GetStyle((int)StandardStyle.SpecialSymbol);//);
                        }
                        else
                            style = line.GetStyle(i, Styles);

                        if (high)
                        {
                            var sstyle = Styles.GetStyle(StandardStyle.Selection);
                            style = sstyle.Combine(style);
                        }

                        style.DrawAll(g, rect, c, pos);

                        if (Buffer.Selections.HasCaret(pos))
                        {
                            var blink = Buffer.Selections.Main.Caret.Line == lineIndex
                                && Buffer.Selections.Main.Caret.Col == i;

                            if (blink)
                            {
                                var cg = CaretRenderer.GetDrawingSurface();
                                cg.Clear(high ? Styles.Selection.BackColor 
                                    : curline ? Settings.CurrentLineIndicatorColor : Styles.Default.BackColor);
                                style.DrawAll(cg, new Rectangle(default(Point), rect.Size), c, pos);

                                if (Settings.LongLineIndicators.Any(ind => ind == i) || (WordWrap && WordWrapColumn == i))
                                {
                                    cg.DrawLine(CachedPen.Create(Styles.SpecialSymbol.ForeColor),
                                        0, 0, 0, rect.Size.Height);
                                }

                                CaretRenderer.Resume();
                            }

                            carets.Add(new CaretData(x, y, pos.Line, pos.Col, blink));
                        }
                    }

                    x += xw;
                }

                var addedWidth = 0;

                if (line.Length > 0 && ShowLineLength)
                    addedWidth = Renderer.DrawLineLengthIndicator(g, line.Length, x, y);

                if (line.Folding.Has(FoldingStates.Header) && lineIndex + 1 < Lines.Count &&
                    Lines[lineIndex + 1].Folding.Has(FoldingStates.Invisible))
                    Renderer.DrawFoldingIndicator(g, x + addedWidth, y);
                
                oldcut = cut;
                y += Info.LineHeight;
                x = lmarg;
            }
        }

        public override Color BackColor => Styles.Default.BackColor;

        public override Color ForeColor => Styles.Default.ForeColor;

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

            var leftMouseDown = e.Button == MouseButtons.Left;
            var p = Locations.LocationToPosition(e.Location);

            if (leftMouseDown && p.IsEmpty)
            {
                if (e.Y - Scroll.ScrollPosition.Y > Info.TextIntegralHeight && Scroll.LastVisibleLine < Lines.Count - 1)
                    p = new Pos(Scroll.LastVisibleLine + 1, Lines[Scroll.LastVisibleLine + 1].Length);
                else if (e.Y < Info.TextTop && Scroll.FirstVisibleLine > 0)
                    p = new Pos(Scroll.FirstVisibleLine - 1, 0);
            }

            if (!p.IsEmpty && Lines[p.Line].Length == p.Col && Folding.IsCollapsedHeader(p.Line))
                Cursor = Cursors.Arrow;
            else if (mlist == null)
                Cursor = Cursors.IBeam;

            mousePosition = e.Location;
            movePosition = p;
            Caret = p;

            if (!p.IsEmpty)
            {
                var keys = ModifierKeys.ToModifiers();
                var mouse = GetMouseClick(e);

                if (mouse != SpecialKey.None || keys != Modifiers.None)
                    Commands.Run(new KeyInput(keys | Modifiers.Move, mouse));

                timer.Start();
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (PointToClient(Cursor.Position) == mousePosition
                && !movePosition.IsEmpty && movePosition.Line < Lines.Count)
                CallTips.MouseDwell(movePosition);
            else
                CallTips.HideCallTip();

            timer.Stop();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

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
            var mouse = GetMouseClick(e);
            
            if (e.Button == MouseButtons.Left)
            {
                var mlist = Locations.FindMargin(e.Location);

                if (mlist != null)
                    mlist.CallMarginMethod(MarginMethod.MouseDown, e.Location);
                else
                {
                    var pos = Locations.LocationToPosition(e.Location);
                    Caret = pos.IsEmpty ? default(Pos) : pos;
                    Commands.Run(new KeyInput(ModifierKeys.ToModifiers(), mouse));
                    var idx = Buffer.Selections.IndexOfCaret(pos);

                    if (idx != -1)
                        Buffer.Selections[idx].SetToRestore();

                    if (Lines[pos.Line].Length == pos.Col && Folding.IsCollapsedHeader(pos.Line))
                    {
                        Folding.ToggleExpand(pos.Line);
                        Scroll.InvalidateLines();
                        Redraw();
                    }
                }
            }

            if (!Focused)
                Focus();
        }
        
        protected override bool ProcessMnemonic(char charCode) =>
            ProcessKey(charCode, ModifierKeys.ToModifiers()) || base.ProcessMnemonic(charCode);

        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR)
                ProcessMnemonic(Convert.ToChar(m.WParam.ToInt32()));

            return base.ProcessKeyMessage(ref m);
        }

        private bool ProcessKey(char ch, Modifiers keys)
        {
            if (keys == Modifiers.None || keys == Modifiers.Shift)
            {
                switch (ch)
                {
                    case '\b':
                    case '\r':
                    case '\t':
                    case '\u001b': //Esc
                        return true;
                    default:
                        new InsertCharCommand(new Character(ch)).Run(this);
                        break;
                }

                return true;
            }

            return false;
        }

        internal void Redraw() => Invalidate();

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
                case Keys.Control | Keys.Right:
                case Keys.Control | Keys.Left:
                case Keys.Control | Keys.Up:
                case Keys.Control | Keys.Down:
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

            if (Autocomplete.WindowShown && Autocomplete.ListenKeys(e.KeyData))
                return;

            var keys = ModifierKeys.ToModifiers();

            if (e.KeyValue == (int)Keys.ControlKey || e.KeyValue == (int)Keys.ShiftKey)
                return;

            var sc = e.KeyCode.ToSpecialKey();
            Commands.Run(sc != SpecialKey.None 
                ? new KeyInput(keys, sc)
                : new KeyInput(keys, GetChar(e)));
        }

        private SpecialKey GetMouseClick(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Left && e.Clicks == 2 ? SpecialKey.DoubleClick 
                : e.Button == MouseButtons.Left ? SpecialKey.Click
                : e.Button == MouseButtons.Right ? SpecialKey.RightClick
                : SpecialKey.None;
        }

        private char GetChar(KeyEventArgs e)
        {
            var keyValue = e.KeyValue;
            if (!e.Shift && keyValue >= (int)Keys.A && keyValue <= (int)Keys.Z)
                return (char)(keyValue + 32);
            return (char)keyValue;
        }

        [Browsable(false)]
        public DocumentBuffer Buffer { get; private set; }

        [Browsable(false)]
        public Document Document => Buffer.Document;

        internal List<Line> Lines => Document.Lines;

        [Browsable(false)]
        public override string Text
        {
            get
            {
                var @lock = Buffer.ObtainLock();

                try
                {
                    return Buffer.GetText();
                }
                finally
                {
                    @lock.Release();
                }
            }
            set
            {
                var @lock = Buffer.ObtainLock();

                try
                {
                    base.Text = value;
                    var doc = Document.Read(value);
                    Buffer = new DocumentBuffer(doc);
                    InitializeBuffer(Buffer);
                    Scroll.InvalidateLines();
                    Styles.RestyleDocument();
                    Folding.RebuildFolding(full: true);
                }
                finally
                {
                    @lock.Release();
                }
            }
        }

        [Browsable(false)]
        public bool Overtype
        {
            get { return Buffer.Overtype; }
            set
            {
                if (value != Buffer.Overtype)
                {
                    Buffer.Overtype = value;
                    CaretRenderer.BlockCaret = value;
                }
            }
        }

        [Browsable(false)]
        public bool WordWrap => Buffer.WordWrap ?? Settings.WordWrap;

        [Browsable(false)]
        public int WordWrapColumn => Buffer.WordWrapColumn ?? Settings.WordWrapColumn;

        [Browsable(false)]
        public bool UseTabs => Buffer.UseTabs ?? Settings.UseTabs;

        [Browsable(false)]
        public int IndentSize => Buffer.IndentSize ?? Settings.IndentSize;

        [Browsable(false)]
        public bool ShowEol => Buffer.ShowEol ?? Settings.ShowEol;

        [Browsable(false)]
        public bool ShowWhitespace => Buffer.ShowWhitespace ?? Settings.ShowWhitespace;

        [Browsable(false)]
        public bool ShowLineLength => Buffer.ShowLineLength ?? Settings.ShowLineLength;

        [Browsable(false)]
        public bool CurrentLineIndicator => Buffer.CurrentLineIndicator ?? Settings.CurrentLineIndicator;

        [Browsable(false)]
        public bool ReadOnly
        {
            get { return Buffer.ReadOnly; }
            set { Buffer.ReadOnly = value; }
        }

        internal IEditorContext Context => this;

        [Browsable(false)]
        public EditorInfo Info { get; }

        [Browsable(false)]
        public EditorSettings Settings { get; }

        [Browsable(false)]
        public MarginList TopMargins { get; }

        [Browsable(false)]
        public MarginList LeftMargins { get; }

        [Browsable(false)]
        public MarginList RightMargins { get; }

        [Browsable(false)]
        public MarginList BottomMargins { get; }

        internal CachedBrush CachedBrush { get; private set; }

        internal CachedPen CachedPen { get; private set; }

        internal CachedFont CachedFont { get; set; }

        internal CachedFont CachedSmallFont { get; set; }

        internal CaretRenderer CaretRenderer { get; }

        internal Renderer Renderer { get; }

        public Pos Caret { get; private set; }

        [Browsable(false)]
        public CommandManager Commands { get; }

        [Browsable(false)]
        public StyleManager Styles { get; }

        [Browsable(false)]
        public ScrollingManager Scroll { get; }

        [Browsable(false)]
        public LocationManager Locations { get; }

        [Browsable(false)]
        public FoldingManager Folding { get; }

        [Browsable(false)]
        public CallTipManager CallTips { get; }

        [Browsable(false)]
        public MatchBracketManager MatchBrackets { get; }

        [Browsable(false)]
        public AutocompleteManager Autocomplete { get; }

        [Browsable(false)]
        public AffinityManager AffinityManager { get; }

        [Browsable(false)]
        public int FirstEditLine { get; set; }

        [Browsable(false)]
        public int LastEditLine { get; set; }
    }
}
