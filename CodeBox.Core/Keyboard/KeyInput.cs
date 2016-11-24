using System;

namespace CodeBox.Core.Keyboard
{
    public sealed class KeyInput : IEquatable<KeyInput>
    {
        public static readonly KeyInput Empty = new KeyInput(Modifiers.None, '\0');

        public KeyInput(Modifiers mod, char key)
        {
            Modifier = mod;
            Key = (int)(char.ToUpper(key));
        }

        public KeyInput(Modifiers mod, SpecialKey key)
        {
            Modifier = mod;
            Key = (int)key;
        }

        public bool IsEmpty()
        {
            return Modifier == Modifiers.None && Key == 0;
        }

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

        public override string ToString()
        {
            var ret = Modifier == Modifiers.None ? KeyToString()
                : $"{Modifier.ToString().Replace(", ", "+")}+{KeyToString()}";
            return Chord == null ? ret : ret + "," + Chord.ToString();
        }

        private string KeyToString()
        {
            return Key >= (int)SpecialKey.Space
                ? KeyboardAdapter.SpecialKeysToString != null ?
                    KeyboardAdapter.SpecialKeysToString[(SpecialKey)Key]
                        : ((SpecialKey)Key).ToString() : ((char)Key).ToString();
        }

        public bool Equals(KeyInput other)
        {
            return Modifier == other.Modifier && Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyInput && Equals((KeyInput)obj);
        }

        public Modifiers Modifier { get; }

        public int Key { get; }

        public KeyInput Chord { get; set; }
    }
}
