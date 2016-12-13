using System;

namespace TaskEngine
{
    public class Argument
    {
        private const string NULL = "<null>";

        private Argument(string name, object value)
        {
            Name = string.IsNullOrEmpty(name) ? null : name;
            Value = value;
        }

        public static Argument FromObject(object value)
        {
            return FromObject(null, value);
        }

        public static Argument FromObject(string name, object value)
        {
            return new Argument(name, value);
        }

        public override string ToString()
        {
            if (Name != null)
                return $"\"{Name}\"=\"{Value}";
            else
                return ($"\"{Value ?? NULL}\"").ToString();
        }

        public string Name { get; }

        public object Value { get; }
    }
}