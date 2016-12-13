using System;

namespace CodeBox.Main.CommandBar
{
    public struct Loc : IEquatable<Loc>
    {
        public Loc(int start, int end)
        {
            Start = start;
            End = end;
        }

        public readonly int Start;
        public readonly int End;

        public bool Equals(Loc other) => Start == other.Start && End == other.End;

        public override bool Equals(object other) => other is Loc ? Equals((Loc)other) : false;

        public override int GetHashCode() => Start.GetHashCode();

        public override string ToString() => $"({Start},{End})";
    }
}
