using System;

namespace ClrSpy.CliSupport
{
    class ErrorWithExitCodeException : Exception
    {
        public int ExitCode { get; }

        public ErrorWithExitCodeException(int exitCode, string message) : base(message)
        {
            ExitCode = exitCode;
        }

        public bool ShowUsage { get; set; }
    }
}