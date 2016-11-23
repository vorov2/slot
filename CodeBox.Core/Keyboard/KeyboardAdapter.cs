using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeBox.Core.Keyboard
{
    using MAP = Dictionary<KeyInput, object>;

    public sealed class KeyboardAdapter
    {
        private readonly MAP inputs = new MAP();
        private MAP lastDict;

        public void RegisterInput(Identifier key, string shortcut)
        {
            Parse(key, shortcut);
        }

        public InputState ProcessInput(KeyInput input)
        {
            var map = lastDict ?? inputs;
            object obj;

            if (!map.TryGetValue(input, out obj))
            {
                lastDict = null;
                return InputState.Unrecognized;
            }
            else if (obj is MAP)
            {
                lastDict = obj as MAP;
                return InputState.Chord;
            }
            else
            {
                lastDict = null;
                LastKey = (Identifier)obj;
                return InputState.Complete;
            }
        }

        private void Parse(Identifier key, string shortcut)
        {
            var arr = shortcut.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var dict = inputs;

            for (var i = 0; i < arr.Length; i++)
            {
                var mod = Modifiers.None;
                var a = arr[i];
                var last = i == arr.Length - 1;
                var arr2 = a.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

                for (var j = 0; j < arr2.Length; j++)
                {
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

                        object obj;
                        var found = dict.TryGetValue(input, out obj);

                        if (found && last)
                            dict[input] = key;
                        else if (found && !last)
                        {
                            var dict2 = obj as MAP;

                            if (dict2 == null)
                                dict2 = new MAP();

                            dict = dict2;
                        }
                        else if (!found && last)
                            dict.Add(input, key);
                        else if (!found && !last)
                        {
                            var dict2 = new MAP();
                            dict.Add(input, dict2);
                            dict = dict2;
                        }
                    }
                }
            }
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
    }
}
