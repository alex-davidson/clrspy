using System;
using ClrSpy.CliSupport;

namespace ClrSpy.Processes
{
    public class ProcessNotFoundException : ErrorWithExitCodeException
    {
        public ProcessNotFoundException(string message) : base(3, message)
        {
        }
    }
}