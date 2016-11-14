using System;

namespace Json
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class JsonIgnoreAttribute : Attribute
    {
        
    }
}