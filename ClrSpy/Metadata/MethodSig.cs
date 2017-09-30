namespace ClrSpy.Metadata
{
    public class MethodSig
    {
        public CallingConvention CallingConvention { get; set; }
        public int GenericParameterCount { get; set; }
        public TypeSig ReturnType { get; set; }
        public ParamSig[] Parameters { get; set; }
    }
}
