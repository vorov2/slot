using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Margins
{
    public sealed class ScrollBarMargin : Margin
    {
        private bool mouseDown;
        private int lastCaretPos;
        private int lastCaretSize;

        public override MarginEffects MouseDown(Point loc, EditorContext ctx)
        {
            if (IsCaretInLocation(ctx, loc))
            {
                mouseDown = true;
                return MarginEffects.Redraw | MarginEffects.CaptureMouse;
            }
            else
            {
                var value = GetScrollValue(loc, ctx);
                SetScrollPosition(ctx, value);
                return MarginEffects.Redraw;
            }
        }

        public override MarginEffects MouseUp(Point loc, EditorContext ctx)
        {
            mouseDown = false;
            return MarginEffects.Redraw;
        }

        public override MarginEffects MouseMove(Point loc, EditorContext ctx)
        {
            if (mouseDown)
            {
                var value = GetScrollValue(loc, ctx);
                SetScrollPosition(ctx, value);
                return MarginEffects.Redraw;
            }

            return MarginEffects.None;
        }

        private int GetScrollValue(Point loc, EditorContext ctx)
        {
            var max = GetMaximum(ctx);
            var v = Horizontal(ctx) ? loc.X : loc.Y;
            var ret = -(max * (v - lastCaretSize / 2) / (GetScrollSize(ctx) - lastCaretSize));

            if (ret > 0)
                ret = 0;
            else if (ret < -max)
                ret = -max;

            return ret;
        }

        public override bool Draw(Graphics g, Rectangle bounds, EditorContext ctx)
        {
            if (Horizontal(ctx))
            {
                g.FillRectangle(ctx.Renderer.Create(Editor.BackgroundColor), bounds);
                    //new Rectangle(bounds.X - ctx.Scroll.X, bounds.Y - ctx.Scroll.Y, bounds.Width, CalculateSize(ctx)));

                var caretSize = ((double)bounds.Width / ctx.Scroll.Width) * bounds.Width;

                if (caretSize < ctx.Info.CharWidth*3)
                    caretSize = ctx.Info.CharWidth*3;

                var perc = (bounds.Width - caretSize) / 100d;
                var curpos = (int)Math.Floor(Math.Floor(ctx.Scroll.X / (ctx.Scroll.Width / 100d)) * perc);

                lastCaretSize = (int)Math.Floor(caretSize);
                g.FillRectangle(ctx.Renderer.Create(mouseDown ? Color.White : Editor.GrayColor),
                    new Rectangle(bounds.X + Math.Abs(curpos), bounds.Y, lastCaretSize, CalculateSize(ctx)));
                    //new Rectangle(bounds.X + Math.Abs(curpos) - ctx.Scroll.X, bounds.Y - ctx.Scroll.Y, lastCaretSize, CalculateSize(ctx)));
                lastCaretPos = bounds.X + Math.Abs(curpos);
            }
            else
            {
                g.FillRectangle(ctx.Renderer.Create(Editor.BackgroundColor), bounds);
                    //new Rectangle(bounds.X - ctx.Scroll.X, bounds.Y - ctx.Scroll.Y, CalculateSize(ctx), bounds.Height));

                var caretSize = ((double)bounds.Height / ctx.Scroll.Height) * bounds.Height;

                if (caretSize < ctx.Info.LineHeight)
                    caretSize = ctx.Info.LineHeight;

                var perc = (bounds.Height - caretSize) / 100d;
                var curpos = (int)Math.Floor(Math.Floor(ctx.Scroll.Y / (ctx.Scroll.Height / 100d)) * perc);

                lastCaretSize = (int)Math.Floor(caretSize);
                g.FillRectangle(ctx.Renderer.Create(mouseDown ? Color.White : Editor.GrayColor),
                    new Rectangle(bounds.X, bounds.Y + Math.Abs(curpos), CalculateSize(ctx), lastCaretSize));
                    //new Rectangle(bounds.X - ctx.Scroll.X, bounds.Y + Math.Abs(curpos) - ctx.Scroll.Y, CalculateSize(ctx), lastCaretSize));
                lastCaretPos = bounds.Y + Math.Abs(curpos);
            }

            return true;
        }

        public override int CalculateSize(EditorContext ctx)
        {
            if (Horizontal(ctx))
            {
                if (ctx.Scroll.Width <= ctx.Info.EditorWidth)
                    return 0;
                else
                    return ctx.Info.CharWidth;
            }
            else
            {
                //if (ctx.Scroll.Height <= ctx.Info.ClientHeight)
                //    return 0;
                //else
                    return ctx.Info.CharWidth;
            }
        }

        private bool IsCaretInLocation(EditorContext ctx, Point loc)
        {
            if (Horizontal(ctx))
                return loc.X >= lastCaretPos && loc.X < lastCaretPos + lastCaretSize;
            else
                return loc.Y >= lastCaretPos && loc.Y < lastCaretPos + lastCaretSize;
        }

        private void SetScrollPosition(EditorContext ctx, int value)
        {
            if (Horizontal(ctx))
                ctx.Editor.SetScrollPositionX(value);
            else
                ctx.Editor.SetScrollPositionY(value);
        }

        private int GetScrollSize(EditorContext ctx)
        {
            if (Horizontal(ctx))
                return ctx.Editor.ClientSize.Width;
            else
                return ctx.Info.EditorHeight;
        }

        private int GetMaximum(EditorContext ctx)
        {
            if (Horizontal(ctx))
                return ctx.Scroll.Width;
            else
                return ctx.Scroll.Height;
        }

        private bool Horizontal(EditorContext ctx)
        {
            return ctx.Editor.BottomMargins.Any(b => b == this);
        }
    }
}
