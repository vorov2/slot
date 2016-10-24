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
    public class Editor : Control, IEditorContext
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
            Caret = new CaretRenderer(this);
            CachedBrush = new CachedBrush();
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

        private void InitializeBuffer(DocumentBuffer buffer)
        {
            buffer.Eol = Settings.Eol;
            buffer.WordWrap = Settings.WordWrap;
        }

        private void Initialize()
        {
            Buffer = new DocumentBuffer(Document.Read(""));
            InitializeBuffer(Buffer);
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

            e.Graphics.FillRectangle(Styles.Default.BackBrush,
                new Rectangle(Info.TextLeft, Info.TextTop, Info.TextWidth, Info.TextHeight));

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            DrawMargins(0, e.Graphics, TopMargins);
            DrawMargins(0, e.Graphics, LeftMargins);

            e.Graphics.TranslateTransform(Scroll.X, Scroll.Y);
            DrawLines(e.Graphics);
#if DEBUG
            // Console.WriteLine($"OnPaint: {call++}; Selections: {Document.Selections.Count}");
            //Console.WriteLine("Time: " + (DateTime.Now - dt).TotalMilliseconds);
#endif


            e.Graphics.ResetTransform();
            DrawMargins(Info.TextBotom, e.Graphics, BottomMargins);
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
            return CopyCommand.GetTextRange(this, range);
        }

        

        private void DrawLines(Graphics g)
        {
            var fvl = Scroll.FirstVisibleLine;
            var lvl = Scroll.LastVisibleLine;
            Console.WriteLine($"FistLine {fvl} and LastLine {lvl}");

            for (var i = fvl; i < lvl + 1; i++)
            {
                var ln = Document.Lines[i];
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
            if (Cursor.Position.X > Info.TextLeft && Cursor.Position.X < Info.TextRight)
                CommandManager.Run<SelectWordCommand>(default(CommandArgument));
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

                if (p.IsEmpty && e.Y - Scroll.Y > Info.TextIntegralHeight)
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

                var start = Buffer.Selections[0].Start;
                var maxLen = 0;

                foreach (var sel in Buffer.Selections)
                {
                    var len = Math.Abs(sel.End.Col - sel.Start.Col);

                    if (len > maxLen)
                        maxLen = len;
                }

                var lastLen = Math.Abs(start.Col - p.Col);

                if (p.Col < lines[p.Line].Length - 1 || lastLen > maxLen)
                    maxLen = lastLen;

                Buffer.Selections.Clear();

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
                            Buffer.Selections.Set(sel);
                        else
                        {
                            var osel = Buffer.Selections.GetSelection(p);
                            if (osel != null)
                                Buffer.Selections.Remove(osel);

                            Buffer.Selections.Add(sel);
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
                            Buffer.Selections.Set(sel);
                        else
                        {
                            var osel = Buffer.Selections.GetSelection(p);

                            if (osel != null)
                                Buffer.Selections.Remove(osel);

                            //Console.WriteLine($"Start: {start};p: {p}");
                            Buffer.Selections.Add(sel);
                        }
                    }
                }

                Scroll.UpdateVisibleRectangle();
                Redraw();
            }
            #endregion
            else if (mouseDown && e.Button == MouseButtons.Left)
            {
                var sel = Buffer.Selections.Main;
                sel.End = p;
                var osel = Buffer.Selections.GetSelection(p, sel);

                if (osel != null)
                    Buffer.Selections.Remove(osel);

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
            if (loc.X < Info.TextLeft)
                return LeftMargins;
            else if (loc.X > Info.TextRight)
                return RightMargins;
            else if (loc.Y < Info.TextTop)
                return TopMargins;
            else if (loc.Y > Info.TextBotom)
                return BottomMargins;
            else
                return null;
        }

        internal Margin mouseThief;

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
                    Buffer.Selections.Add(sel);
                else
                    Buffer.Selections.Set(sel);

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
            var stripe = (int)Math.Ceiling((loc.Y - Info.TextTop - line.Y - Scroll.Y) / (double)Info.LineHeight) - 1;
            var cut = line.GetCut(stripe);
            var sc = stripe > 0 ? line.GetCut(stripe - 1) + 1 : 0;
            var width = Info.TextLeft;
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
                        var arg = new CommandArgument(ch, null);
                        CommandManager.Run<InsertCharCommand>(arg);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Insert)
                Overtype = !Overtype;

            lastKeys = e.Modifiers;
            CommandManager.Run(lastKeys | e.KeyCode, default(CommandArgument));
        }

        internal int FindLineByLocation(int locY)
        {
            var line = Lines[Scroll.FirstVisibleLine];
            var lineIndex = 0;
            var y = Info.TextTop;
            locY = locY - Scroll.Y;
            //var maxHeight = Info.EditorIntegralHeight; //Round by line height

            do
            {
                var lh = line.Stripes * Info.LineHeight;

                if (locY >= y && locY <= y + lh)// && locY <= maxHeight)
                    return lineIndex;

                if (Lines.Count == lineIndex + 1)
                    return -1;

                y += lh;
                line = Lines[++lineIndex];
            } while (true);
        }

        public void Copy()
        {
            CommandManager.Run<CopyCommand>(default(CommandArgument));
        }

        public void Cut()
        {
            CommandManager.Run<CutCommand>(default(CommandArgument));
        }

        public void Paste()
        {
            CommandManager.Run<PasteCommand>(default(CommandArgument));
        }

        public void Undo()
        {
            CommandManager.Undo();
        }

        public void Redo()
        {
            CommandManager.Redo();
        }

        public void SelectAll()
        {
            CommandManager.Run<SelectAllCommand>(default(CommandArgument));
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

        public EditorInfo Info { get; private set; }

        public EditorSettings Settings { get; }

        public MarginList TopMargins { get; private set; }

        public MarginList LeftMargins { get; private set; }

        public MarginList RightMargins { get; private set; }

        public MarginList BottomMargins { get; private set; }

        internal CachedBrush CachedBrush { get; private set; }

        internal CachedFont CachedFont { get; set; }

        internal CaretRenderer Caret { get; private set; }
        
        internal CommandManager CommandManager { get; private set; }

        public StyleManager Styles { get; private set; }

        public ScrollingManager Scroll { get; private set; }
    }
}
