using System;
using System.Text;
using System.Collections.Generic;

namespace StringMacro
{
    public sealed class MacroParser
    {
        private readonly VariableProviders vars;
        private readonly Dictionary<string, string> dict;
        
        public MacroParser(Dictionary<string, string> dict) : this(dict, VariableProviders.Empty)
        {

        }

        public MacroParser(Dictionary<string, string> dict, VariableProviders vars)
        {
            this.dict = dict;
            this.vars = vars;
        }

        public string Parse(string source)
        {
            var buffer = source.ToCharArray();
            var sb = new StringBuilder();

            for (var i = 0; i < buffer.Length; i++)
            {
                var c = buffer[i];
                var eof = i == buffer.Length - 1;

                if (c == '{' && !eof && buffer[i + 1] != '{')
                    i = ParseMacro(i + 1, buffer, sb);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private int ParseMacro(int i, char[] buffer, StringBuilder sb)
        {
            var sb2 = new StringBuilder();
            var prefix = default(string);

            for (; i < buffer.Length; i++)
            {
                var c = buffer[i];
                var eof = i == buffer.Length - 1;
                
                if (c == '.')
                {
                    prefix = sb2.ToString();
                    sb2.Clear();
                }
                else if (c == '}' && (eof || buffer[i + 1] != '}'))
                {
                    var key = sb2.ToString();
                    string val;
                    IVariableProvider mac;

                    if (prefix != null && (mac = vars.GetProvider(prefix)) != null
                        && mac.TryResolve(key, out val))
                        sb.Append(val);
                    else if (dict.TryGetValue(key, out val))
                        sb.Append(val);
                    else
                    {
                        sb.Append('{');

                        if (prefix != null)
                        {
                            sb.Append(prefix);
                            sb.Append('.');
                        }

                        sb.Append(key);
                        sb.Append('}');
                    }

                    return i;
                }
                else
                    sb2.Append(c);
            }

            return buffer.Length;
        }
    }
}