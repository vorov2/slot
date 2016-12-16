using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Editor.ObjectModel
{
    public struct Pos : IComparable<Pos>
    {
        public static readonly Pos Empty = new Pos(-1, -1);

        public Pos(int line, int col)
        {
            Line = line;
            Col = col;
        }

        public readonly int Line;
        public readonly int Col;

        public bool IsEmpty => Line == -1 && Col == -1;
        
        public static bool operator !=(Pos lho, Pos rho) => Compare(lho, rho) != 0;

        public static bool operator ==(Pos lho, Pos rho) => Compare(lho, rho) == 0;

        public static bool operator >(Pos lho, Pos rho) => Compare(lho, rho) == 1;

        public static bool operator <(Pos lho, Pos rho) => Compare(lho, rho) == -1;

        public static bool operator >=(Pos lho, Pos rho) => Compare(lho, rho) >= 0;

        public static bool operator <=(Pos lho, Pos rho) => Compare(lho, rho) <= 0;

        public static int Compare(Pos lho, Pos rho) =>
            lho.Line > rho.Line || (lho.Line == rho.Line && lho.Col > rho.Col) ? 1 :
                lho.Line == rho.Line && lho.Col == rho.Col ? 0 : -1;

        public int CompareTo(Pos other) => Compare(this, other);

        public override bool Equals(object obj) => obj is Pos && CompareTo((Pos)obj) == 0;

        public override string ToString() => $"Line={Line};Col={Col}";

        public override int GetHashCode() => (Line << 22 | Col << 10).GetHashCode();
    }
}
