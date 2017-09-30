using System;

namespace ClrSpy.Metadata
{
    public class CustomAttributeSig
    {
        public FixedArg[] FixedArgs { get; set; }
        public NamedArg[] NamedArgs { get; set; }
    }

    public struct FixedArg
    {
        public Type Type { get; set; }
        public object Value { get; set; }
    }

    public struct NamedArg
    {
        public bool IsProperty { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}
