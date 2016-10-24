using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Styling
{
    public struct StyleId : IEquatable<StyleId>, IEquatable<int>
    {
        public readonly static int MinValue = 100;
        public readonly static int MaxValue = int.MaxValue;

        public StyleId(int value)
        {
            if (value < MinValue)
                throw new ArgumentException(
                    $"Value {value} is too low for style ID. Minimum allowed value is {MinValue}.", "value");

            Value = value;
        }

        public static implicit operator int(StyleId obj)
        {
            return obj.Value;
        }

        public static implicit operator StyleId(int i)
        {
            return new StyleId(i);
        }

        public override bool Equals(object obj)
        {
            return obj is StyleId ? ((StyleId)obj).Value == Value
                : obj is int ? (int)obj == Value
                : false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(StyleId other)
        {
            return Value == other.Value;
        }

        public bool Equals(int other)
        {
            return Value == other;
        }
        
        public int Value { get; }
    }
}
