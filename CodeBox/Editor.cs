//#define BIG
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

namespace CodeBox
{
    public class Editor : ScrollableControl
    {
        internal Size FontSize { get; private set; }
        internal Font font;

        public Editor()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.IBeam;
            AutoScroll = true;
            TabStop = true;
            LeftMargins = new MarginList(this);
            Info = new EditorInfo(this);
            Caret = new EditorCaret(this);
            context = new EditorContext(this);
            Initialize();
        }

        internal static Color ForegroundColor = ColorTranslator.FromHtml("#DCDCDC");
        internal static Color BackgroundColor = ColorTranslator.FromHtml("#1E1E1E");
        internal static Color HighlightColor = ColorTranslator.FromHtml("#264F78");
        internal static Color GrayColor = ColorTranslator.FromHtml("#505050");
        //

        private static Brush _background;
        internal static Brush Background
        {
            get
            {
                if (_background == null)
                    _background = new SolidBrush(BackgroundColor);
                return _background;
            }
        }

        private static Brush _foreground;
        internal static Brush Foreground
        {
            get
            {
                if (_foreground == null)
                    _foreground = new SolidBrush(ForegroundColor);
                return _foreground;
            }
        }

        private static Brush _highlight;
        internal static Brush Highlight
        {
            get
            {
                if (_highlight == null)
                    _highlight = new SolidBrush(HighlightColor);
                return _highlight;
            }
        }

        private static Brush _gray;
        internal static Brush Gray
        {
            get
            {
                if (_gray == null)
                    _gray = new SolidBrush(GrayColor);
                return _gray;
            }
        }

        private void Initialize()
        {
            if (FontSize.IsEmpty)
            {
                using (var g = CreateGraphics())
                {
                    var fs = 11f;
                    font = new Font("Consolas", fs, FontStyle.Regular, GraphicsUnit.Point);
                    //var size1 = TextRenderer.MeasureText(e.Graphics, "<F>", font);
                    //var size2 = TextRenderer.MeasureText(e.Graphics, "<>", font);
                    var size1 = g.MeasureString("<F>", font);
                    var size2 = g.MeasureString("<>", font);
                    FontSize = new Size((int)(size1.Width - size2.Width), (int)font.GetHeight(g));
                    HorizontalScroll.SmallChange = Info.CharWidth;
                    VerticalScroll.SmallChange = Info.LineHeight;
                }
            }

            Document = new Document();
            //Settings.WordWrap = true;

            CommandManager = new CommandManager(this);

            CommandManager.Register<InsertCharCommand>();
            CommandManager.Register<SelectWordCommand>();
            CommandManager.Register<CutCommand>();
            CommandManager.Register<CopyCommand>();
            CommandManager.Register<PasteCommand>();
            CommandManager.Register<SelectAllCommand>();

            CommandManager.Register<ShiftTabCommand>(Keys.Shift|Keys.Tab);
            CommandManager.Register<TabCommand>(Keys.Tab);
            CommandManager.Register<ClearSelectionCommand>(Keys.Escape);
            CommandManager.Register<LeftCommand>(Keys.Left);
            CommandManager.Register<RightCommand>(Keys.Right);
            CommandManager.Register<UpCommand>(Keys.Up);
            CommandManager.Register<DownCommand>(Keys.Down);
            CommandManager.Register<HomeCommand>(Keys.Home);
            CommandManager.Register<EndCommand>(Keys.End);
            CommandManager.Register<InsertNewLineCommand>(Keys.Enter);
            CommandManager.Register<DeleteBackCommand>(Keys.Back);
            CommandManager.Register<DeleteCommand>(Keys.Delete);
            CommandManager.Register<PageDownCommand>(Keys.PageDown);
            CommandManager.Register<PageUpCommand>(Keys.PageUp);
            CommandManager.Register<ExtendLeftCommand>(Keys.Shift | Keys.Left);
            CommandManager.Register<ExtendRightCommand>(Keys.Shift | Keys.Right);
            CommandManager.Register<ExtendUpCommand>(Keys.Shift | Keys.Up);
            CommandManager.Register<ExtendDownCommand>(Keys.Shift | Keys.Down);
            CommandManager.Register<ExtendEndCommand>(Keys.Shift | Keys.End);
            CommandManager.Register<ExtendHomeCommand>(Keys.Shift | Keys.Home);
            CommandManager.Register<WordLeftCommand>(Keys.Control | Keys.Left);
            CommandManager.Register<WordRightCommand>(Keys.Control | Keys.Right);
            CommandManager.Register<ExtendWordRightCommandCommand>(Keys.Control | Keys.Shift | Keys.Right);
            CommandManager.Register<ExtendWordLeftCommandCommand>(Keys.Control | Keys.Shift | Keys.Left);
            CommandManager.Register<ExtendPageDownCommand>(Keys.Control | Keys.PageDown);
            CommandManager.Register<ExtendPageUpCommand>(Keys.Control | Keys.PageUp);
            CommandManager.Register<DocumentHomeCommand>(Keys.Control | Keys.Home);
            CommandManager.Register<DocumentEndCommand>(Keys.Control | Keys.End);
            CommandManager.Register<ExtendDocumentHomeCommand>(Keys.Control | Keys.Shift | Keys.Home);
            CommandManager.Register<ExtendDocumentEndCommand>(Keys.Control | Keys.Shift | Keys.End);
        }

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_ENDSCROLL = 0x8;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL)
                if (m.WParam.ToInt32() != SB_ENDSCROLL)
                    Redraw();

            base.WndProc(ref m);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Caret.Suspend();

            var dt = DateTime.Now;

            BackColor = BackgroundColor;
            e.Graphics.FillRectangle(Background,
                new Rectangle(Info.LeftMargin, Info.TopMargin, Info.ClientWidth, Info.ClientHeight));

            e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var w = 0;

            foreach (var m in LeftMargins)
            {
                var bounds = new Rectangle(w, Info.TopMargin, m.Width, Info.ClientHeight);
                m.Draw(e.Graphics, bounds, GetEditorContext());
                w += m.Width;
            }

            //DrawSelections(e.Graphics);
            DrawLines(e.Graphics);
#if DEBUG
            // Console.WriteLine($"OnPaint: {call++}; Selections: {Document.Selections.Count}");
            Console.WriteLine("Time: " + (DateTime.Now - dt).TotalMilliseconds);
#endif

            base.OnPaint(e);
        }

        private void BuildLines(string source)
        {
            OriginalEol = source.IndexOf("\r\n") != -1 ? Eol.CrLf
                : source.IndexOf("\n") != -1 ? Eol.Lf
                : Eol.Cr;

            var txt = source.Replace("\r\n", "\n").Replace('\r', '\n');

            foreach (var ln in txt.Split('\n'))
                Document.Lines.Add(Line.FromString(ln, ++Document.LineSequence));
        }

        protected override void OnResize(EventArgs eventargs)
        {
            if (Settings.WordWrap)
                InvalidateLines();

            base.OnResize(eventargs);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);

            Redraw();
            //InvalidateScrollbars();
            //Update();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Redraw();
        }

        private int GetX(Pos pos)
        {
            return Info.LeftMargin + Lines[pos.Line].GetTetras(pos.Col, Settings.TabSize) * Info.CharWidth;
        }

        StringFormat sf = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };


        internal void InvalidateLines()
        {
            var dt = DateTime.Now;
            var idx = 0;

            if (!Settings.WordWrap)
            {
                var maxWidth = 0;
                var y = 0;

                foreach (var ln in Document.Lines)
                {
                    ln.Index = idx++;
                    ln.Y = y;
                    var w = ln.GetTetras(Settings.TabSize) * Info.CharWidth;
                    y += Info.LineHeight;

                    if (w > maxWidth)
                        maxWidth = w;
                }

                var linesPerScreen = Info.ClientHeight / Info.LineHeight - 1;
                AutoScrollMinSize = new Size(maxWidth, Document.Lines.Count * Info.LineHeight + linesPerScreen * Info.LineHeight);
            }
            else
            {
                var maxHeight = 0;
                var twidth = Info.ClientWidth;

                foreach (var ln in Document.Lines)
                {
                    ln.Index = idx++;
                    ln.RecalculateCuts(twidth, Info.CharWidth, Settings.TabSize);
                    ln.Y = maxHeight;
                    maxHeight += ln.Stripes * Info.LineHeight;
                }

                AutoScrollMinSize = new Size(0, maxHeight);
            }


            Console.WriteLine("Invalidate: " + (DateTime.Now - dt).TotalMilliseconds);
        }

        private void DrawLines(Graphics g)
        {
            foreach (var ln in Document.Lines)
            {
                var lnStart = Info.TopMargin + ln.Y;
                var lnEnd = lnStart + ln.Stripes * Info.LineHeight;

                if (lnEnd > Info.TopMargin - AutoScrollPosition.Y
                    && lnStart < Info.ClientHeight - AutoScrollPosition.Y)
                {
                    DrawLine(g, ln);
                }
            }
        }

        private void DrawLine(Graphics g, Line line)
        {
            var lmarg = Info.LeftMargin;
            var x = lmarg;
            var y = Info.TopMargin + line.Y;
            var oldcut = 0;
            var sel = IsLineSelected(line);

            for (var j = 0; j < line.Stripes; j++)
            {
                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                for (var i = oldcut; i < cut; i++)
                {
                    var c = line.CharAt(i);

                    var col = c == ' ' || c == '\t' || c == '\0' ? Gray : Foreground;
                    var str = c == ' ' && Settings.ShowWhitespace ? "·" : c.ToString();
                    str = c == '\t' && Settings.ShowWhitespace ? "\u2192" : str;
                    str = c == '\0' && Settings.ShowEol ? "\u00B6" : str;
                    var xw = c == '\t' ? Settings.TabSize * Info.CharWidth : Info.CharWidth;
                    var high = sel && Document.Selections.IsSelected(new Pos(line.Index, i));

                    if (high)
                        g.FillRectangle(Highlight, new Rectangle(x, y, xw, Info.LineHeight));

                    if (x + AutoScrollPosition.X >= lmarg)
                        g.DrawString(str, font, col, new PointF(x, y), sf);

                    if (Document.Selections.HasCaret(new Pos(line.Index, i)))
                    {
                        var blink = Document.Selections.Main.Caret.Line == line.Index && Document.Selections.Main.Caret.Col == i;

                        if (blink)
                        {
                            var cg = Caret.GetDrawingSurface();
                            cg.Clear(high ? HighlightColor : BackgroundColor);
                            cg.DrawString(str, font, col, new Point(0, 0), sf);
                            Caret.Resume();
                        }

                        Caret.DrawCaret(g, x, y, blink);
                    }
                    x += xw;
                }

                oldcut = cut;
                y += Info.LineHeight;
                x = lmarg;
            }
        }

        private void DrawSelections(Graphics g)
        {
            var dt = DateTime.Now;
            foreach (var s in Document.Selections)
            {
                if (s.IsEmpty)
                    continue;

                var sel = s.Normalize();
                var lmarg = Info.LeftMargin;

                for (var i = sel.Start.Line; i < sel.End.Line + 1; i++)
                {
                    var line = Document.Lines[i];
                    var y = line.Y;

                    if (line.Stripes == 1)
                    {
                        var sx = i == sel.Start.Line ? line.GetTetras(sel.Start.Col, Settings.TabSize) : 0;
                        var ex = i == sel.End.Line ? line.GetTetras(sel.End.Col, Settings.TabSize) : line.GetTetras(Settings.TabSize) + 1;
                        sx = lmarg + sx * Info.CharWidth - AutoScrollPosition.X;
                        ex = lmarg + ex * Info.CharWidth;
                        g.FillRectangle(Highlight, new Rectangle(sx, y, ex - sx, Info.LineHeight));
                    }
                    else
                    {
                        var oldcut = 0;

                        for (var j = 0; j < line.Stripes; j++)
                        {
                            var cut = line.GetCut(j);
                            var instart = sel.Start.Line == i && sel.Start.Col <= cut && sel.Start.Col > oldcut;
                            var inend = sel.End.Line == i && sel.End.Col <= cut && sel.End.Col > oldcut;
                            var inside = (i != sel.Start.Line && i != sel.End.Line)
                                || (i == sel.Start.Line && i != sel.End.Line && cut > sel.Start.Col)
                                || (i == sel.End.Line && i != sel.Start.Line && cut < sel.End.Col);

                            if (inside || instart || inend)
                            {
                                var sx = instart
                                    ? lmarg + (line.GetTetras(sel.Start.Col, Settings.TabSize) * Info.CharWidth - oldcut * Info.CharWidth)
                                    : lmarg;
                                var wi = inend
                                    ? lmarg + (line.GetTetras(sel.End.Col, Settings.TabSize) * Info.CharWidth - oldcut * Info.CharWidth) - sx
                                    : line.GetTetras(cut, Settings.TabSize) * Info.CharWidth - oldcut * Info.CharWidth - sx
                                        + Info.CharWidth * (cut == line.Length ? 2 : 1);
                                g.FillRectangle(Highlight, new Rectangle(sx, y, wi, Info.LineHeight));
                            }

                            y += Info.LineHeight;
                            oldcut = cut;
                        }
                    }
                }
            }
        }

        private bool IsLineSelected(Line line)
        {
            foreach (var s in Document.Selections)
            {
                var sel = s.Normalize();

                if (!s.IsEmpty && line.Index >= sel.Start.Line && line.Index <= sel.End.Line)
                    return true;
            }

            return false;
        }


        private int IsLineStripeVisible(Pos pos)
        {
            var ln = Lines[pos.Line];
            var stripe = ln.GetStripe(pos.Col);
            var cy = Info.TopMargin + ln.Y + stripe * Info.LineHeight + AutoScrollPosition.Y;
            return cy < 0 ? cy / Info.LineHeight - 1 :
                   cy + Info.LineHeight > Info.ClientHeight ? cy / Info.LineHeight :
                   0;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            var ctx = GetEditorContext();
            CommandManager.Run<SelectWordCommand>(ctx);
        }

        //Range sel;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.X < Info.LeftMargin)
                Cursor = Cursors.Arrow;
            else
                Cursor = Cursors.IBeam;

            var p = default(Pos);

            if (mouseDown)
            {
                var ln = FindLineByLocation(e.Location);
                p = ln != null ? FindTextLocation(ln, e.Location) : Pos.Empty;

                //Test if we need to scroll down in selection mode
                if (e.Y - AutoScrollPosition.Y > Info.IntegralHeight)
                {
                    ln = FindLineByLocation(new Point(e.Location.X, Info.IntegralHeight + AutoScrollPosition.Y));
                    p = FindTextLocation(ln, e.Location);

                    if (Document.Lines.Count > ln.Index)
                    {
                        p = new Pos(ln.Index + 1, ln.Length <= p.Col ? p.Col : ln.Length);
                        Redraw();
                    }
                }
            }

            if (p.IsEmpty)
                return;

            #region Block selection
            if (mouseDown && e.Button == MouseButtons.Left && ((lastKeys & Keys.Alt) == Keys.Alt))
            {
                var lines = Document.Lines;

                if (lines[p.Line].Length == 0)
                    return;

                var start = Document.Selections[0].Start;
                var maxLen = 0;

                foreach (var sel in Document.Selections)
                {
                    var len = Math.Abs(sel.End.Col - sel.Start.Col);

                    if (len > maxLen)
                        maxLen = len;
                }

                var lastLen = Math.Abs(start.Col - p.Col);

                if (p.Col < lines[p.Line].Length - 1 || lastLen > maxLen)
                    maxLen = lastLen;

                Document.Selections.ForceClear();

                if (start > p)
                {
                    for (var i = start.Line; i > p.Line - 1; i--)
                    {
                        var ln = lines[i];

                        if (ln.Length == 0)
                            continue;

                        var sel = new Selection(
                            new Pos(i, start.Col),
                            new Pos(i, start.Col + maxLen > ln.Length ? ln.Length : start.Col + maxLen));

                        if (i == start.Line)
                            Document.Selections.Set(sel);
                        else
                        {
                            var osel = Document.Selections.GetSelection(p);
                            if (osel != null)
                                Document.Selections.Remove(osel);

                            Document.Selections.Add(sel);
                        }
                    }
                }
                else
                {
                    for (var i = start.Line; i < p.Line + 1; i++)
                    {
                        var ln = lines[i];

                        if (ln.Length == 0)
                            continue;

                        var endCol = p.Col < start.Col
                            ? (start.Col - maxLen < 0 ? 0 : start.Col - maxLen)
                            : (start.Col + maxLen > ln.Length ? ln.Length : start.Col + maxLen);
                        var sel = new Selection(new Pos(i, start.Col), new Pos(i, endCol));

                        if (i == start.Line)
                            Document.Selections.Set(sel);
                        else
                        {
                            var osel = Document.Selections.GetSelection(p);

                            if (osel != null)
                                Document.Selections.Remove(osel);

                            Console.WriteLine($"Start: {start};p: {p}");
                            Document.Selections.Add(sel);
                        }
                    }
                }

                UpdateVisibleRectangle();
                Redraw();
            }
            #endregion
            else if (mouseDown && e.Button == MouseButtons.Left)
            {
                var sel = Document.Selections.Main;
                sel.End = p;
                var osel = Document.Selections.GetSelection(p, sel);

                if (osel != null)
                    Document.Selections.Remove(osel);

                UpdateVisibleRectangle();
                Redraw();
            }
        }


        private void ScrollVertical(int lines)
        {
            var val = VerticalScroll.Value + Info.LineHeight * lines;

            if (val < VerticalScroll.Minimum)
                val = VerticalScroll.Minimum;
            else if (val > VerticalScroll.Maximum)
                val = VerticalScroll.Maximum;

            VerticalScroll.Value = val;
            InvalidateScrollbars();
        }
        private void InvalidateScrollbars()
        {
            base.AutoScrollMinSize -= new Size(1, 0);
            base.AutoScrollMinSize += new Size(1, 0);
        }
        internal bool UpdateVisibleRectangle()
        {
            var caret = Document.Selections.Main.Caret;
            var sv = IsLineStripeVisible(caret);
            var update = false;

            if (sv != 0)
            {
                var sign = Math.Sign(sv);

                if (Math.Abs(sv) > 1 && IsLineStripeVisible(new Pos(caret.Line - sign, caret.Col)) == 0)
                    sv = sign;

                ScrollVertical(sv);
                update = true;
            }

            if (!Settings.WordWrap)
            {
                var xpos = GetX(caret);
                var p = xpos + AutoScrollPosition.X;

                if (p >= Info.ClientWidth || p <= Info.LeftMargin)
                {
                    HorizontalScroll.Value = xpos - Info.LeftMargin;
                    update = true;
                }

                InvalidateScrollbars();

                if (HorizontalScroll.Value < HorizontalScroll.Minimum)
                    HorizontalScroll.Value = HorizontalScroll.Minimum;
            }

            return update;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
        }

        bool mouseDown;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            mouseDown = true;
            var ln = FindLineByLocation(e.Location);

            if (e.X < Info.LeftMargin)
            {
                foreach (var m in LeftMargins)
                    m.Click(e.X, e.Y, ln, GetEditorContext());

                UpdateVisibleRectangle();
                Redraw();
            }
            else
            {
                var pos = FindTextLocation(ln, e.Location);
                pos = pos.IsEmpty ? default(Pos) : pos;
                var sel = new Selection(pos, pos);

                if ((lastKeys & Keys.Control) == Keys.Control)
                    Document.Selections.Add(sel);
                else
                    Document.Selections.Set(sel);

                Redraw();
            }

            if (!Focused)
                Focus();
        }

        private Pos FindTextLocation(Line ln, Point loc)
        {
            if (ln != null)
            {
                var col = LocateColumn(ln, loc);
                return col == -1 ? Pos.Empty : new Pos(ln.Index, col);
            }

            return Pos.Empty;
        }

        private int LocateColumn(Line line, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - Info.TopMargin - line.Y - AutoScrollPosition.Y) / (double)Info.LineHeight) - 1;
            var cut = line.GetCut(stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) + 1 : 0;
            var width = Info.LeftMargin;
            var app = Info.CharWidth * .15;

            for (var i = sc; i < cut + 1; i++)
            {
                var c = line.CharAt(i);
                var cw = c == '\t' ? Settings.TabSize * Info.CharWidth : Info.CharWidth;

                if (loc.X >= width - app && loc.X <= width + cw - app)
                    return i;

                width += cw;
            }

            return loc.X > width ? line.Length : 0;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            return ProcessKey(charCode, lastKeys) || base.ProcessMnemonic(charCode);
        }

        const int WM_CHAR = 0x102;

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
                        var ctx = GetEditorContext();
                        ctx.Char = ch;
                        CommandManager.Run<InsertCharCommand>(ctx);
                        break;
                }

                Redraw();
                return true;
            }

            return false;
        }

        private Keys lastKeys;
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.ShiftKey)
                lastKeys &= ~Keys.Shift;
            if (e.KeyCode == Keys.Alt)
                lastKeys &= ~Keys.Alt;
            if (e.KeyCode == Keys.ControlKey)
                lastKeys &= ~Keys.Control;
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

        private readonly EditorContext context;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Insert)
                Overtype = !Overtype;

            lastKeys = e.Modifiers;
            var ctx = GetEditorContext();
            CommandManager.Run(lastKeys | e.KeyCode, ctx);
        }

        private Line FindLineByLocation(Point loc)
        {
            var line = Lines[0];
            var y = 0;
            var locY = loc.Y - AutoScrollPosition.Y;
            var maxHeight = Info.IntegralHeight; //Round by line height

            do
            {
                var lh = line.Stripes * Info.LineHeight;

                if (locY >= y && locY <= y + lh && locY <= maxHeight)
                    return line;

                if (Lines.Count == line.Index + 1)
                    return null;

                y += lh;
                line = Lines[line.Index + 1];
            } while (true);
        }

        public void Copy()
        {
            CommandManager.Run<CopyCommand>(GetEditorContext());
        }

        public void Cut()
        {
            CommandManager.Run<CutCommand>(GetEditorContext());
        }

        public void Paste()
        {
            CommandManager.Run<PasteCommand>(GetEditorContext());
        }

        public void Undo()
        {
            CommandManager.Undo(GetEditorContext());
        }

        public void Redo()
        {
            CommandManager.Redo(GetEditorContext());
        }

        public void SelectAll()
        {
            CommandManager.Run<SelectAllCommand>(GetEditorContext());
        }

        public Document Document { get; private set; }

        internal List<Line> Lines
        {
            get { return Document.Lines; }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                base.Text = value;


                BuildLines(Text);
                InvalidateLines();
            }
        }

        internal EditorContext GetEditorContext()
        {
            return context;
        }

        private bool _overtype;
        public bool Overtype
        {
            get { return _overtype; }
            set
            {
                if (value != _overtype)
                {
                    _overtype = value;
                    Caret.BlockCaret = value;
                    Redraw();
                }
            }
        }

        public EditorInfo Info { get; private set; }

        private EditorSettings _settings;
        public EditorSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new EditorSettings();

                return _settings;
            }
        }

        public MarginList LeftMargins { get; private set; }

        internal EditorCaret Caret { get; private set; }
        
        internal CommandManager CommandManager { get; private set; }

        internal Eol OriginalEol { get; private set; }
    }

}
