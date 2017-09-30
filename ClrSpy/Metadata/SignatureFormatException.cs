using System;

namespace ClrSpy.Metadata
{
    public class SignatureFormatException : Exception
    {
        public int Offset { get; }

        public SignatureFormatException(string message, int offset) : base(message)
        {
            Offset = offset;
        }
    }
}
