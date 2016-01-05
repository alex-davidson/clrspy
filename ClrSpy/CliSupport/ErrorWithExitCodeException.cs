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

        private ErrorWithExitCodeException(int exitCode) : base("")
        {
            ExitCode = exitCode;
        }

        public static ErrorWithExitCodeException Propagate(int exitCode)
        {
            return new ErrorWithExitCodeException(exitCode);
        }

        public bool ShowUsage { get; set; }
    }
}