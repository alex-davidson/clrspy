using System;
using System.Diagnostics;
using ClrSpy.CliSupport;

namespace ClrSpy.Architecture
{
    class Requires64BitEnvironmentException : InvalidOperationException
    {
        public Requires64BitEnvironmentException() : base("The requested operation requires running as a 64-bit process.")
        {
            Debug.Assert(!Environment.Is64BitProcess, "Demanded 64-bit invocation but already running as 64-bit!");
            Debug.Assert(Environment.Is64BitOperatingSystem, "Demanded 64-bit invocation but OS does not support it!");
        }
    }
}