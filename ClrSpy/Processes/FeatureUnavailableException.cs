using System;

namespace ClrSpy.Processes
{
    public class FeatureUnavailableException : NotSupportedException
    {
        public FeatureUnavailableException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}