using System;
using System.Diagnostics;

namespace ClrSpy.Architecture
{
    class Requires32BitEnvironmentException : InvalidOperationException
    {
        public Requires32BitEnvironmentException() : base("The requested operation requires running as a 32-bit process.")
        {
            Debug.Assert(Environment.Is64BitProcess, "Demanded 32-bit invocation but already running as 32-bit!");
        }
    }
}