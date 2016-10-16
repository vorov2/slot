using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
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

        public bool IsEmpty
        {
            get { return Line == -1 && Col == -1; }
        }
        
        public static bool operator !=(Pos lho, Pos rho)
        {
            return Compare(lho, rho) != 0;
        }

        public static bool operator ==(Pos lho, Pos rho)
        {
            return Compare(lho, rho) == 0;
        }

        public static bool operator >(Pos lho, Pos rho)
        {
            return Compare(lho, rho) == 1;
        }

        public static bool operator <(Pos lho, Pos rho)
        {
            return Compare(lho, rho) == -1;
        }

        public static bool operator >=(Pos lho, Pos rho)
        {
            return Compare(lho, rho) >= 0;
        }

        public static bool operator <=(Pos lho, Pos rho)
        {
            return Compare(lho, rho) <= 0;
        }

        public static int Compare(Pos lho, Pos rho)
        {
            return lho.Line > rho.Line || (lho.Line == rho.Line && lho.Col > rho.Col) ? 1 :
                lho.Line == rho.Line && lho.Col == rho.Col ? 0 : -1;
        }

        public int CompareTo(Pos other)
        {
            return Compare(this, other);
        }

        public override bool Equals(object obj)
        {
            return obj is Pos && CompareTo((Pos)obj) == 0;
        }

        public override string ToString()
        {
            return String.Format("Line={0};Col={1}", Line, Col);
        }

        public override int GetHashCode()
        {
            return (Line << 22 | Col << 10).GetHashCode();
        }
    }
}
