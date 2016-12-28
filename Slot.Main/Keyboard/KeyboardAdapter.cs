using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Json;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.Packages;
using Slot.Core;
using Slot.Core.Keyboard;
using Slot.Core.Output;

namespace Slot.Main.Keyboard
{
    using Core.Settings;
    using Core.ViewModel;
    using MAP = Dictionary<KeyInput, Identifier>;

    [Export(typeof(IKeyboardAdapter))]
    [ComponentData(Name)]
    public sealed class KeyboardAdapter : IKeyboardAdapter
    {
        public const string Name = "keyboard.default";

        private readonly MAP inputs = new MAP();
        private readonly Dictionary<KeyInput, object> chords = new Dictionary<KeyInput, object>();
        private readonly Dictionary<Identifier, KeyInput> shortcuts = new Dictionary<Identifier, KeyInput>();
        private readonly Dictionary<Identifier, KeymapMetadata> keymaps = new Dictionary<Identifier, KeymapMetadata>();
        private KeyInput currentChord;
        private volatile bool loaded;
        private Identifier currentKeymap;

        [Import]
        private IPackageManager packageManager = null;

        private KeyboardAdapter()
        {

        }

        public IEnumerable<KeymapMetadata> EnumerateKeymaps()
        {
            EnsureRead();
            return keymaps.Values;
        }

        private void EnsureRead()
        {
            if (loaded)
                return;

            foreach (var pkg in packageManager.EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Keymaps))
                {
                    var key = (Identifier)e.String("key");
                    keymaps.Add(
                        key,
                        new KeymapMetadata(
                            key,
                            e.String("name"),
                            new FileInfo(Path.Combine(pkg.Directory.FullName, "data", e.String("file")))
                        ));
                }

            var set = App.Component<IViewManager>().ActiveView.Settings.Get<EnvironmentSettings>();
            //set.SettingsChanged += (o, ev) =>
            //{
            //    var kid = (Identifier)set.Keymap;

            //    if (currentKeymap != kid)
            //        ChangeKeymap(kid);
            //};
            ChangeKeymap((Identifier)set.Keymap);
            loaded = true;
        }

        private void ChangeKeymap(Identifier key)
        {
            KeymapMetadata km;

            if (!keymaps.TryGetValue(key, out km))
            {
                App.Ext.Log($"Unknown keymap: {key}", EntryType.Error);
                return;
            }

            string content;

            if (!FileUtil.ReadFile(km.File, Encoding.UTF8, out content))
                return;

            KeymapReader.Read(content, this);
            currentKeymap = key;
        }

        public void RegisterInput(Identifier key, string shortcut)
        {
            var chord = Parse(key, shortcut);
            shortcuts.Remove(key);
            shortcuts.Add(key, chord);

            var cmd = App.Component<ICommandProvider>().GetCommandByKey(key);
            if (cmd != null)
                cmd.Shortcut = chord.ToString();
        }

        public KeyInput GetCommandShortcut(Identifier key)
        {
            EnsureRead();
            KeyInput ret;
            shortcuts.TryGetValue(key, out ret);
            return ret;
        }

        public InputState ProcessInput(KeyInput input)
        {
            EnsureRead();
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
        private static Dictionary<SpecialKey, string> specialKeysToString;

        private static bool Eq(string fst, string snd)
        {
            return string.Equals(fst, snd, StringComparison.OrdinalIgnoreCase);
        }

        private static SpecialKey ParseSpecialKey(string value)
        {
            if (specialKeys == null)
            {
                specialKeys = new Dictionary<string, SpecialKey>(StringComparer.OrdinalIgnoreCase);
                specialKeysToString = new Dictionary<SpecialKey, string>();
                var fields = typeof(SpecialKey).GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var fi in fields)
                {
                    var nam = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute))
                        ?.ToString() ?? fi.Name;
                    var val = (SpecialKey)fi.GetValue(null);
                    specialKeys.Add(nam, val);
                    specialKeysToString.Add(val, nam);
                }
            }

            SpecialKey ret;

            if (specialKeys.TryGetValue(value, out ret))
                return ret;

            return SpecialKey.None;
        }

        public string KeyInputToString(KeyInput input)
        {
            var ret = input.Modifier == Modifiers.None ? KeyToString(input.Key)
                : $"{input.Modifier.ToString().Replace(", ", "+")}+{KeyToString(input.Key)}";
            return input.Chord == null ? ret : ret + "," + input.Chord.ToString();
        }

        private string KeyToString(int key)
        {
            return key >= (int)SpecialKey.Space
                ? specialKeysToString != null ?
                    specialKeysToString[(SpecialKey)key]
                        : ((SpecialKey)key).ToString() : ((char)key).ToString();
        }

        public Identifier LastKey { get; private set; }
    }
}
