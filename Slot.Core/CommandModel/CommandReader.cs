using Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core.CommandModel
{
    public static class CommandReader
    {
        public static IEnumerable<CommandMetadata> Read(string source)
        {
            var p = new JsonParser(source);
            var obj = p.Parse();

            var list = obj as List<object>;

            if (list != null)
                foreach (var o in list)
                {
                    var dict = o as Dictionary<string, object>;
                    if (dict != null)
                        yield return ReadCommand(dict);
                }
            else
            {
                var dict = obj as Dictionary<string, object>;

                if (dict != null)
                    yield return ReadCommand(dict);
            }
        }

        private static CommandMetadata ReadCommand(Dictionary<string, object> dict)
        {
            var met = new CommandMetadata
            {
                Key = (Identifier)dict.String("key"),
                Mode = (Identifier)dict.String("mode"),
                Alias = dict.String("alias"),
                Title = dict.String("title"),
                Shortcut = dict.String("shortcut")
            };

            var obj = dict.Object("arguments") as List<object>;

            if (obj != null)
            {
                foreach (var a in obj)
                {
                    var d = a as Dictionary<string, object>;
                    if (d != null)
                        met.Arguments.Add(ReadArgument(d));
                }
            }

            return met;
        }

        private static ArgumentMetadata ReadArgument(Dictionary<string, object> dict)
        {
            return new ArgumentMetadata
            {
                Name = dict.String("name"),
                ValueProvider = (Identifier)dict.String("valueProvider"),
                Optional = dict.Bool("optional"),
                Affinity = dict.Enum<ArgumentAffinity>("affinity")
            };
        }
    }
}
