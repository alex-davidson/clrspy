using System;

namespace ClrSpy.Processes
{
    public class ProcessNotSpecifiedException : ArgumentException
    {
        public ProcessNotSpecifiedException() : base("No process specified.")
        {
        }
    }
}