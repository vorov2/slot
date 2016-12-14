using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeBox.Core.Keyboard
{
    using CommandModel;
    using MAP = Dictionary<KeyInput, Identifier>;

    public sealed class KeyboardAdapter
    {
        private readonly MAP inputs = new MAP();
        private readonly Dictionary<KeyInput, object> chords = new Dictionary<KeyInput, object>();
        private readonly Dictionary<Identifier, KeyInput> shortcuts = new Dictionary<Identifier, KeyInput>();
        private KeyInput currentChord;

        private KeyboardAdapter()
        {

        }

        public void RegisterInput(Identifier key, string shortcut)
        {
            var chord = Parse(key, shortcut);
            shortcuts.Remove(key);
            shortcuts.Add(key, chord);

            var cmd = App.Catalog<ICommandProvider>().Default().GetCommandByKey(key);
            if (cmd != null)
                cmd.Shortcut = chord.ToString();
        }

        public KeyInput GetCommandShortcut(Identifier key)
        {
            KeyInput ret;
            shortcuts.TryGetValue(key, out ret);
            return ret;
        }

        public InputState ProcessInput(KeyInput input)
        {
            var key = default(Identifier);

            if (!inputs.TryGetValue(input, out key))
            {
                if (chords.ContainsKey(input))
                {
                    currentChord = input;
                    return InputState.Chord;
                }
                else
                    return InputState.Unrecognized;
            }

            LastKey = key;
            return InputState.Complete;
        }

        private KeyInput Parse(Identifier key, string shortcut)
        {
            var arr = shortcut.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            KeyInput chord = null;

            for (var i = 0; i < arr.Length; i++)
            {
                var mod = Modifiers.None;
                var a = arr[i];
                var last = i == arr.Length - 1;
                var arr2 = a.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

                for (var j = 0; j < arr2.Length; j++)
                {
                    if (chord != null && !chords.ContainsKey(chord))
                        chords.Add(chord, null);

                    var ch = arr2[j];

                    if (j != arr2.Length - 1)
                    {
                        mod |=
                              Eq(ch, "Ctrl") ? Modifiers.Ctrl
                            : Eq(ch, "Alt") ? Modifiers.Alt
                            : Eq(ch, "Shift") ? Modifiers.Shift
                            : Eq(ch, "Win") ? Modifiers.Cmd
                            : Eq(ch, "Cmd") ? Modifiers.Cmd
                            : Eq(ch, "Move") ? Modifiers.Move
                            : Modifiers.None;
                    }
                    else
                    {
                        var input = ch.Length == 1 ? new KeyInput(mod, ch[0])
                            : new KeyInput(mod, ParseSpecialKey(ch));

                        if (j == arr2.Length - 1)
                            inputs.Add(chord ?? input, key);

                        if (chord == null)
                            chord = input;
                        else
                        {
                            chord.Chord = input;
                            chord = input;
                        }
                    }
                }
            }

            return chord;
        }

        private static Dictionary<string, SpecialKey> specialKeys;
        internal static Dictionary<SpecialKey, string> SpecialKeysToString { get; private set; }

        private static bool Eq(string fst, string snd)
        {
            return string.Equals(fst, snd, StringComparison.OrdinalIgnoreCase);
        }

        private static SpecialKey ParseSpecialKey(string value)
        {
            if (specialKeys == null)
            {
                specialKeys = new Dictionary<string, SpecialKey>(StringComparer.OrdinalIgnoreCase);
                SpecialKeysToString = new Dictionary<SpecialKey, string>();
                var fields = typeof(SpecialKey).GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var fi in fields)
                {
                    var nam = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute))
                        ?.ToString() ?? fi.Name;
                    var val = (SpecialKey)fi.GetValue(null);
                    specialKeys.Add(nam, val);
                    SpecialKeysToString.Add(val, nam);
                }
            }

            SpecialKey ret;

            if (specialKeys.TryGetValue(value, out ret))
                return ret;

            return SpecialKey.None;
        }

        public Identifier LastKey { get; private set; }

        public static KeyboardAdapter Instance = new KeyboardAdapter();
    }
}
