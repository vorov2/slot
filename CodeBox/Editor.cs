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
using CodeBox.Drawing;
using CodeBox.Styling;

namespace CodeBox
{
    public class Editor : Control
    {
        public Editor()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.IBeam;
            TabStop = true;
            TopMargins = new MarginList(this);
            LeftMargins = new MarginList(this);
            RightMargins = new MarginList(this);
            BottomMargins = new MarginList(this);
            Info = new EditorInfo(this);
            Caret = new EditorCaret(this);
            CachedBrush = new CachedBrush();
            context = new EditorContext(this);
            Scroll = new ScrollingManager(this);
            Styles = new StyleManager(this);
            Settings = new EditorSettings(this);
            Initialize();
            
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
                
        private void Initialize()
        {
            Document = new Document();
            //Settings.WordWrap = true;

            CommandManager = new CommandManager(this);

            CommandManager.Register<InsertCharCommand>();
            CommandManager.Register<SelectWordCommand>();
            CommandManager.Register<CutCommand>();
            CommandManager.Register<CopyCommand>();
            CommandManager.Register<PasteCommand>();
            CommandManager.Register<SelectAllCommand>();

            CommandManager.Register<ShiftTabCommand>(Keys.Shift | Keys.Tab);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            Caret.Suspend();

            var dt = DateTime.Now;

            e.Graphics.FillRectangle(Styles.BackBrush(StandardStyle.Default),
                new Rectangle(Info.EditorLeft, Info.EditorTop, Info.EditorWidth, Info.EditorHeight));

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            DrawMargins(0, e.Graphics, TopMargins);
            DrawMargins(0, e.Graphics, LeftMargins);

            //DrawSelections(e.Graphics);
            e.Graphics.TranslateTransform(Scroll.X, Scroll.Y);
            DrawLines(e.Graphics);
#if DEBUG
            // Console.WriteLine($"OnPaint: {call++}; Selections: {Document.Selections.Count}");
            //Console.WriteLine("Time: " + (DateTime.Now - dt).TotalMilliseconds);
#endif


            e.Graphics.ResetTransform();
            if (BottomMargins.Any())
                DrawMargins(ClientSize.Height -
                    BottomMargins.First().CalculateSize(), e.Graphics, BottomMargins);

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
                    bounds = new Rectangle(start, Info.EditorTop, m.CalculateSize(), ClientSize.Height - Info.EditorTop);
                else if (top)
                    bounds = new Rectangle(0, start, ClientSize.Width, m.CalculateSize());
                else
                    bounds = new Rectangle(Info.EditorLeft, start, Info.EditorWidth, m.CalculateSize());

                m.Draw(g, bounds);
                start += vertical ? m.CalculateSize() : -m.CalculateSize();
            }
        }

        private void BuildLines(string source)
        {
            OriginalEol = source.IndexOf("\r\n") != -1 ? Eol.CrLf
                : source.IndexOf("\n") != -1 ? Eol.Lf
                : Eol.Cr;
            Document.Lines.Clear();
            var txt = source.Replace("\r\n", "\n").Replace('\r', '\n');

            foreach (var ln in txt.Split('\n'))
                Document.Lines.Add(Line.FromString(ln, ++Document.LineSequence));

            Scroll.FirstVisibleLine = Scroll.FirstVisibleLine;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            Scroll.InvalidateLines();
            Styles.Restyle();
            Invalidate();
            base.OnResize(eventargs);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Scroll.ScrollY((e.Delta / 120) * 2);
        }
        

        
        
        public string GetTextRange(Range range)
        {
            return CopyCommand.GetTextRange(GetEditorContext(), range);
        }

        

        private void DrawLines(Graphics g)
        {
            var fvl = Scroll.FirstVisibleLine;
            var lvl = Scroll.LastVisibleLine;

            for (var i = fvl; i < lvl + 1; i++)
            {
                var ln = Document.Lines[i];
                DrawLine(g, ln, i);
            }

            Console.WriteLine($"Last line draw: {lvl}");
        }

        private void DrawLine(Graphics g, Line line, int lineIndex)
        {
            var lmarg = Info.EditorLeft;
            var tmarg = Info.EditorTop;
            var cwidth = Info.EditorRight;
            var x = lmarg;
            var y = tmarg + line.Y;
            var oldcut = 0;
            var sel = Document.Selections.IsLineSelected(lineIndex);

            for (var j = 0; j < line.Stripes; j++)
            {
                var cut = line.GetCut(j);

                if (cut == line.Length)
                    cut++;

                for (var i = oldcut; i < cut; i++)
                {
                    var c = line.CharAt(i);
                    //var style = c != '\0' ? GetStyle(line[i].Style) : defaultStyle;
                    var style = c != '\0' ? line[i].Style : (byte)0;

                    if (c == '\0' || c == '\t' || c == ' ')
                    {
                        style = (byte)StandardStyle.SpecialSymbol;
                        if (c == '\0' && Settings.ShowEol) c = '\u00B6';
                        else if (c == '\t' && Settings.ShowWhitespace) c = '\u2192';
                        else if (c == ' ' && Settings.ShowWhitespace) c = '·';
                    }

                    var xw = c == '\t' ? Settings.TabSize * Info.CharWidth : Info.CharWidth;
                    var visible = x + Scroll.X >= lmarg && x + Scroll.X + xw <= cwidth;
                    var high = visible && sel && Document.Selections.IsSelected(new Pos(lineIndex, i));

                    //if (visible && (high || style.BackColor != null))
                    //    g.FillRectangle(CachedBrush.Create(high ? HighlightColor : style.BackColor.Value),
                    //        new Rectangle(x, y, xw, Info.LineHeight));

                    //var fnt = style.FontStyle != null ? CachedFont.Create(font.Name, font.Size, style.FontStyle.Value) : font;
                    //if (visible)
                    //    g.DrawString(str, fnt, CachedBrush.Create(col), new PointF(x, y), sf);

                    var rect = new RectangleF(x, y, xw, Info.LineHeight);

                    if (visible)
                        Styles.Draw(g, c, style, rect, high);

                    if (visible && Document.Selections.HasCaret(new Pos(lineIndex, i)))
                    {
                        var blink = Document.Selections.Main.Caret.Line == lineIndex
                            && Document.Selections.Main.Caret.Col == i;

                        if (blink)
                        {
                            var cg = Caret.GetDrawingSurface();
                            if (!high)
                                cg.Clear(BackColor);
                            //cg.DrawString(str, fnt, CachedBrush.Create(col), new Point(0, 0), sf);
                            Styles.Draw(cg, c, style, new RectangleF(0, 0, rect.Width, rect.Height), high);
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
                var lmarg = Info.EditorLeft;

                for (var i = sel.Start.Line; i < sel.End.Line + 1; i++)
                {
                    var line = Document.Lines[i];
                    var y = line.Y;

                    if (line.Stripes == 1)
                    {
                        var sx = i == sel.Start.Line ? line.GetTetras(sel.Start.Col, Settings.TabSize) : 0;
                        var ex = i == sel.End.Line ? line.GetTetras(sel.End.Col, Settings.TabSize) : line.GetTetras(Settings.TabSize) + 1;
                        sx = lmarg + sx * Info.CharWidth - Scroll.X;
                        ex = lmarg + ex * Info.CharWidth;
                        g.FillRectangle(CachedBrush.Create(Settings.SelectionColor), new Rectangle(sx, y, ex - sx, Info.LineHeight));
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
                                g.FillRectangle(CachedBrush.Create(Settings.SelectionColor), new Rectangle(sx, y, wi, Info.LineHeight));
                            }

                            y += Info.LineHeight;
                            oldcut = cut;
                        }
                    }
                }
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            var ctx = GetEditorContext();
            
            if (Cursor.Position.X > Info.EditorLeft && Cursor.Position.X < ClientSize.Width - Info.RightMargin)
                CommandManager.Run<SelectWordCommand>(ctx);
        }

        //Range sel;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseThief != null)
            {
                mouseThief.MouseMove(e.Location);
                return;
            }

            var mlist = FindMargin(e.Location);

            if (mlist != null)
            {
                mlist.CallMarginMethod(MarginMethod.MouseMove, e.Location);
                Cursor = Cursors.Arrow;
            }
            else
                Cursor = Cursors.IBeam;

            if (!mouseDown)
                return;

            var p = default(Pos);

            if (mouseDown && e.Button == MouseButtons.Left)
            {
                var lineIndex = FindLineByLocation(e.Location.Y);
                p = lineIndex != -1 ? FindTextLocation(lineIndex, e.Location) : Pos.Empty;

                if (p.IsEmpty && e.Y - Scroll.Y > Info.EditorIntegralHeight)
                {
                    lineIndex = Scroll.LastVisibleLine;
                    var ln = Document.Lines[lineIndex];
                    p = new Pos(lineIndex + 1, ln.Length <= p.Col ? p.Col : ln.Length);
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

                            //Console.WriteLine($"Start: {start};p: {p}");
                            Document.Selections.Add(sel);
                        }
                    }
                }

                Scroll.UpdateVisibleRectangle();
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

                Console.WriteLine(sel);
                Scroll.UpdateVisibleRectangle();
                Redraw();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;

            if (mouseThief != null)
            {
                mouseThief.MouseUp(e.Location);
                mouseThief = null;
                Redraw();
            }

            var mlist = FindMargin(e.Location);

            if (mlist != null)
                mlist.CallMarginMethod(MarginMethod.MouseUp, e.Location);
        }

        private MarginList FindMargin(Point loc)
        {
            if (loc.X < Info.EditorLeft)
                return LeftMargins;
            else if (loc.X > ClientSize.Width - Info.RightMargin)
                return RightMargins;
            else if (loc.Y < Info.EditorTop)
                return TopMargins;
            else if (loc.Y > ClientSize.Height - Info.BottomMargin)
                return BottomMargins;
            else
                return null;
        }

        internal Margin mouseThief;
        private bool MouseMargins(Point loc, MarginList margins, int start, MarginMethod meth)
        {
            var ctx = GetEditorContext();

            foreach (var m in margins)
            {
                var sel = margins == LeftMargins || margins == RightMargins
                    ? loc.X >= start && loc.X <= start + m.CalculateSize()
                    : loc.Y >= start && loc.Y <= start + m.CalculateSize();

                if (sel)
                {
                    var effect = 
                        meth == MarginMethod.MouseDown ? m.MouseDown(loc)
                        : meth == MarginMethod.MouseUp ? m.MouseUp(loc)
                        : m.MouseMove(loc);

                    if ((effect & MarginEffects.Invalidate) == MarginEffects.Invalidate)
                        Scroll.InvalidateLines();
                    if ((effect & MarginEffects.Redraw) == MarginEffects.Redraw)
                        Redraw();
                    if ((effect & MarginEffects.Scroll) == MarginEffects.Scroll)
                        Scroll.UpdateVisibleRectangle();
                    if ((effect & MarginEffects.CaptureMouse) == MarginEffects.CaptureMouse)
                        mouseThief = m;

                    return true;
                }

                start += m.CalculateSize();
            }

            return false;
        }

        bool mouseDown;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            mouseDown = true;
            var mlist = FindMargin(e.Location);

            if (mlist != null)
                mlist.CallMarginMethod(MarginMethod.MouseDown, e.Location);
            else
            {
                var lineIndex = FindLineByLocation(e.Location.Y);
                var pos = FindTextLocation(lineIndex, e.Location);
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

        private Pos FindTextLocation(int lineIndex, Point loc)
        {
            if (lineIndex != -1)
            {
                var col = FindColumnByLocation(Lines[lineIndex], loc);
                return col == -1 ? Pos.Empty : new Pos(lineIndex, col);
            }

            return Pos.Empty;
        }

        private int FindColumnByLocation(Line line, Point loc)
        {
            var stripe = (int)Math.Ceiling((loc.Y - Info.EditorTop - line.Y - Scroll.Y) / (double)Info.LineHeight) - 1;
            var cut = line.GetCut(stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) + 1 : 0;
            var width = Info.EditorLeft;
            var locX = loc.X - Scroll.X;
            var app = Info.CharWidth * .15;

            for (var i = sc; i < cut + 1; i++)
            {
                var c = line.CharAt(i);
                var cw = c == '\t' ? Settings.TabSize * Info.CharWidth : Info.CharWidth;

                if (locX >= width - app && locX <= width + cw - app)
                    return i;

                width += cw;
            }

            return locX > width ? line.Length : 0;
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

        internal int FindLineByLocation(int locY)
        {
            var line = Lines[Scroll.FirstVisibleLine];
            var lineIndex = 0;
            var y = Info.EditorTop;
            locY = locY - Scroll.Y;
            var maxHeight = Info.EditorIntegralHeight; //Round by line height

            do
            {
                var lh = line.Stripes * Info.LineHeight;

                if (locY >= y && locY <= y + lh && locY <= maxHeight)
                    return lineIndex;

                if (Lines.Count == lineIndex + 1)
                    return -1;

                y += lh;
                line = Lines[++lineIndex];
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
            get { return base.Text; }
            set
            {
                base.Text = value;
                BuildLines(Text);
                Scroll.InvalidateLines();
                Styles.Restyle();
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

        public EditorSettings Settings { get; }

        public MarginList TopMargins { get; private set; }

        public MarginList LeftMargins { get; private set; }

        public MarginList RightMargins { get; private set; }

        public MarginList BottomMargins { get; private set; }

        internal CachedBrush CachedBrush { get; private set; }

        internal CachedFont CachedFont { get; set; }

        internal EditorCaret Caret { get; private set; }
        
        internal CommandManager CommandManager { get; private set; }

        public StyleManager Styles { get; private set; }

        public ScrollingManager Scroll { get; private set; }

        internal Eol OriginalEol { get; private set; }
    }
}
