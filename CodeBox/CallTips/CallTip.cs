using CodeBox.ObjectModel;
using System;

namespace CodeBox.CallTips
{
    public struct CallTip : IEquatable<CallTip>
    {
        public static readonly CallTip Empty = new CallTip(null, Pos.Empty, Pos.Empty);

        public CallTip(string data, Pos start, Pos end)
        {
            Data = data;
            Start = start;
            End = end;
        }

        public readonly string Data;

        public readonly Pos Start;

        public readonly Pos End;

        public override string ToString() => $"{{Data={Data};Start={Start};End={End}}}";

        public static bool Equals(CallTip fst, CallTip snd) =>
            fst.Data == snd.Data && fst.Start == snd.Start && fst.End == snd.End;

        public override bool Equals(object obj) => obj is CallTip ? Equals(this, (CallTip)obj) : false;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Data.GetHashCode();
                hash = hash * 23 + Start.GetHashCode();
                hash = hash * 23 + End.GetHashCode();
                return hash;
            }
        }

        public bool Equals(CallTip obj) => Equals(this, obj);

        public static bool operator ==(CallTip fst, CallTip snd) => Equals(fst, snd);

        public static bool operator !=(CallTip fst, CallTip snd) => !Equals(fst, snd);
    }
}
