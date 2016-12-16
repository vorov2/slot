using System;
using System.Collections.Generic;
using System.Drawing;

namespace Slot.Core.Themes
{
    public struct Style : IEquatable<Style>
    {
        public static readonly Style Empty = default(Style);

        public Style(Color foreColor, Color backColor, Color adornmentColor, Adornment adornment, FontStyle fontStyle)
        {
            ForeColor = foreColor;
            BackColor = backColor;
            AdornmentColor = adornmentColor;
            Adornment = adornment;
            FontStyle = fontStyle;
        }

        public Color ForeColor { get; }

        public Color BackColor { get; }

        public Color AdornmentColor { get; }

        public Adornment Adornment { get; }

        public FontStyle FontStyle { get; }

        public Style Combine(Style other) =>
            new Style(
                ForeColor.IsEmpty ? other.ForeColor : ForeColor,
                BackColor.IsEmpty ? other.BackColor : BackColor,
                AdornmentColor.IsEmpty ? other.AdornmentColor : AdornmentColor,
                Adornment == Adornment.None ? other.Adornment : Adornment,
                FontStyle == FontStyle.Regular ? other.FontStyle : FontStyle);

        public bool IsEmpty() => Equals(Empty);

        public override bool Equals(object obj) => obj is Style && Equals((Style)obj);

        public bool Equals(Style other) =>
                   ForeColor == other.ForeColor
                && BackColor == other.BackColor
                && AdornmentColor == other.AdornmentColor
                && Adornment == other.Adornment
                && FontStyle == other.FontStyle;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ForeColor.GetHashCode();
                hash = hash * 23 + BackColor.GetHashCode();
                hash = hash * 23 + AdornmentColor.GetHashCode();
                hash = hash * 23 + Adornment.GetHashCode();
                hash = hash * 23 + FontStyle.GetHashCode();
                return hash;
            }
        }

        public override string ToString() =>
            $"ForeColor: {ForeColor}; BackColor: {BackColor}; AdornmentColor: {AdornmentColor}; Adornment: {Adornment}; FontStyle: {FontStyle}";
    }
}
