using System;
using System.Windows.Forms;

namespace CodeBox.Core.Keyboard
{
    public static class KeysExtensions
    {
        public static KeyInput GetKeyInput(this KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.ControlKey || e.KeyValue == (int)Keys.ShiftKey)
                return KeyInput.Empty;

            var keys = e.KeyData.ToModifiers();
            var sc = e.KeyCode.ToSpecialKey();
            return sc != SpecialKey.None
                ? new KeyInput(keys, sc)
                : new KeyInput(keys, e.GetChar());
        }

        public static char GetChar(this KeyEventArgs e)
        {
            var keyValue = e.KeyValue;
            if (!e.Shift && keyValue >= (int)Keys.A && keyValue <= (int)Keys.Z)
                return (char)(keyValue + 32);
            return (char)keyValue;
        }

        public static SpecialKey GetSpecialKey(this MouseEventArgs e)
        {
            return e.Button == MouseButtons.Left && e.Clicks == 2 ? SpecialKey.DoubleClick
                : e.Button == MouseButtons.Left ? SpecialKey.Click
                : e.Button == MouseButtons.Right ? SpecialKey.RightClick
                : SpecialKey.None;
        }

        public static Modifiers ToModifiers(this Keys keys)
        {
            var ret = Modifiers.None;

            if ((keys & Keys.Control) == Keys.Control)
                ret |= Modifiers.Ctrl;
            if ((keys & Keys.Alt) == Keys.Alt)
                ret |= Modifiers.Alt;
            if ((keys & Keys.Shift) == Keys.Shift)
                ret |= Modifiers.Shift;
            if ((keys & Keys.LWin) == Keys.LWin)
                ret |= Modifiers.Cmd;

            return ret;
        }

        public static SpecialKey ToSpecialKey(this Keys keys)
        {
            switch (keys)
            {
                case Keys.Space: return SpecialKey.Space;
                case Keys.Delete: return SpecialKey.Del;
                case Keys.Tab: return SpecialKey.Tab;
                case Keys.Home: return SpecialKey.Home;
                case Keys.End: return SpecialKey.End;
                case Keys.Insert: return SpecialKey.Ins;
                case Keys.Back: return SpecialKey.Back;
                case Keys.PageUp: return SpecialKey.PageUp;
                case Keys.PageDown: return SpecialKey.PageDown;
                case Keys.Return: return SpecialKey.Enter;
                case Keys.Escape: return SpecialKey.Esc;
                case Keys.F1: return SpecialKey.F1;
                case Keys.F2: return SpecialKey.F2;
                case Keys.F3: return SpecialKey.F3;
                case Keys.F4: return SpecialKey.F4;
                case Keys.F5: return SpecialKey.F5;
                case Keys.F6: return SpecialKey.F6;
                case Keys.F7: return SpecialKey.F7;
                case Keys.F8: return SpecialKey.F8;
                case Keys.F9: return SpecialKey.F9;
                case Keys.F10: return SpecialKey.F10;
                case Keys.F11: return SpecialKey.F11;
                case Keys.F12: return SpecialKey.F12;
                case Keys.Up: return SpecialKey.Up;
                case Keys.Down: return SpecialKey.Down;
                case Keys.Left: return SpecialKey.Left;
                case Keys.Right: return SpecialKey.Right;
                case Keys.OemPeriod: return SpecialKey.Greater;
                case Keys.Oemcomma: return SpecialKey.Lesser;
                case Keys.Oemplus: return SpecialKey.Equal;
                case Keys.OemMinus: return SpecialKey.Minus;
                case Keys.OemQuestion: return SpecialKey.Question;
                case Keys.Oem5: return SpecialKey.Slash;
                case Keys.OemOpenBrackets: return SpecialKey.LeftBracket;
                case Keys.Oem6: return SpecialKey.RightBracket;
                case Keys.Oem7: return SpecialKey.Quote;
                case Keys.Oem1: return SpecialKey.Semicolon;
                case Keys.Oemtilde: return SpecialKey.Tilde;
                default: return SpecialKey.None;
            }
        }
    }
}
