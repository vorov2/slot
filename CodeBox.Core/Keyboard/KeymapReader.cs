using System;
using System.Collections.Generic;

namespace CodeBox.Core.Keyboard
{
    public static class KeymapReader
    {
        public static void Read(string source, KeyboardAdapter adapter)
        {
            var dict = new Json.JsonParser(source).Parse() as Dictionary<string, object>;

            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    var lst = kv.Value as List<object>;

                    if (lst != null)
                    {
                        foreach (var o in lst)
                        {
                            var str = o as string;

                            if (str != null)
                                adapter.RegisterInput(kv.Key, str);
                        }
                    }
                    else
                    {
                        var str = kv.Value as string;
                        if (str != null)
                            adapter.RegisterInput(kv.Key, str);
                    }
                }
            }
        }
    }
}
