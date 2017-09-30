namespace ClrSpy.Metadata
{
    public class PropertySig
    {
        public CallingConvention CallingConvention { get; set; }
        public MetadataToken[] CustomModifiers { get; set; }
        public TypeSig Type { get; set; }
        public ParamSig[] Parameters { get; set; }
    }
}
