using System;

namespace Slot.Core.Keyboard
{
    public sealed class KeyInput : IEquatable<KeyInput>
    {
        public static readonly KeyInput Empty = new KeyInput(Modifiers.None, '\0');

        public KeyInput(Modifiers mod, char key)
        {
            Modifier = mod;
            Key = char.ToUpper(key);
        }

        public KeyInput(Modifiers mod, SpecialKey key)
        {
            Modifier = mod;
            Key = (int)key;
        }

        public bool IsEmpty() => Modifier == Modifiers.None && Key == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Modifier.GetHashCode();
                hash = hash * 23 + Key.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => App.Catalog<IKeyboardAdapter>().Default().KeyInputToString(this);

        public bool Equals(KeyInput other) => Modifier == other.Modifier && Key == other.Key;

        public override bool Equals(object obj) => obj is KeyInput && Equals((KeyInput)obj);

        public Modifiers Modifier { get; }

        public int Key { get; }

        public KeyInput Chord { get; set; }
    }
}
