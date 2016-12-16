using Slot.Core.Themes;
using System;

namespace Slot.Editor.Styling
{
    public struct AppliedStyle : IEquatable<AppliedStyle>
    {
        public static readonly AppliedStyle Empty = new AppliedStyle(StandardStyle.Default, -1, -1);

        public AppliedStyle(StandardStyle styleId, int start, int end)
        {
            StyleId = styleId;
            Start = start;
            End = end;
        }

        public readonly StandardStyle StyleId;

        public readonly int Start;

        public readonly int End;

        public override string ToString() => $"{{StyleId={StyleId};Start={Start};End={End}}}";

        public static bool Equals(AppliedStyle fst, AppliedStyle snd) =>
            fst.StyleId == snd.StyleId && fst.Start == snd.Start && fst.End == snd.End;

        public override bool Equals(object obj) => obj is AppliedStyle ? Equals(this, (AppliedStyle)obj) : false;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + StyleId.GetHashCode();
                hash = hash * 23 + Start.GetHashCode();
                hash = hash * 23 + End.GetHashCode();
                return hash;
            }
        }

        public bool Equals(AppliedStyle obj) => Equals(this, obj);

        public static bool operator ==(AppliedStyle fst, AppliedStyle snd) => Equals(fst, snd);

        public static bool operator !=(AppliedStyle fst, AppliedStyle snd) => !Equals(fst, snd);
    }
}
