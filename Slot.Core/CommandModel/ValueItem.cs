using System;

namespace Slot.Core.CommandModel
{
    public class ValueItem : IEquatable<ValueItem>
    {
        public ValueItem(string text) : this(text, null)
        {
        }

        public ValueItem(string text, string meta)
        {
            Value = text;
            Meta = meta;
        }

        protected ValueItem()
        {

        }

        public virtual string Value { get; }

        public virtual string Meta { get; }

        public override string ToString() => (Value ?? "").ToString();

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Value ?? "").GetHashCode();
                hash = hash * 23 + (Meta ?? "").GetHashCode();
                return hash;
            }
        }

        public static bool Equals(ValueItem fst, ValueItem snd) =>
            fst == snd || fst != null && snd != null && fst.Value == snd.Value && fst.Meta == snd.Meta;

        public bool Equals(ValueItem other) => Equals(this, other);

        public override bool Equals(object obj) => Equals(this, obj as ValueItem);
    }
}
