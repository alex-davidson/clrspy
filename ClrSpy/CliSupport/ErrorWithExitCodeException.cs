using System;

namespace ClrSpy.CliSupport
{
    public class ErrorWithExitCodeException : Exception
    {
        public int ExitCode { get; }

        public ErrorWithExitCodeException(int exitCode, string message) : base(message)
        {
            ExitCode = exitCode;
        }

        public ErrorWithExitCodeException(int exitCode, Exception exception) : base(exception.Message)
        {
            ExitCode = exitCode;
        }

        public bool ShowUsage { get; set; }
    }
}