namespace ClrSpy.Metadata
{
    public struct ParamSig
    {
        public MetadataToken[] CustomModifiers { get; set; }
        public bool IsTypedByRef { get; set; }
        public bool IsByRef { get; set; }
        public TypeSig Type { get; set; }
    }
}
