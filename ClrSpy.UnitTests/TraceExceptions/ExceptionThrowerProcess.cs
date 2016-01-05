using System;
using System.Diagnostics;

namespace ClrSpy.UnitTests.TraceExceptions
{
    public class ExceptionThrowerProcess : IDisposable
    {
        private readonly ProcessStartInfo startInfo;
        private Process process;
        private bool hasTriggered;

        public ExceptionThrowerProcess(string messageToThrow) : this()
        {
            if (messageToThrow == null) throw new ArgumentNullException(nameof(messageToThrow));
            startInfo.Arguments = '\"' + messageToThrow.Replace("\"", "\"\"") + '\"';
        }

        public ExceptionThrowerProcess()
        {
            var throwerApplication = "ExceptionThrower.exe";
            startInfo  = new ProcessStartInfo {
                RedirectStandardInput = true,
                FileName = throwerApplication,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        public Process Start()
        {
            if (process != null) throw new InvalidOperationException();
            process = Process.Start(startInfo);
            return process;
        }

        public void TriggerException()
        {
            if (hasTriggered) throw new InvalidOperationException();
            hasTriggered = true;
            // Write a single character.
            // ExceptionThrower.exe will then continue and throw an exception.
            process.StandardInput.WriteLine();
            process.StandardInput.Flush();
        }

        public void Dispose()
        {
            try
            {
                process.Refresh();
                process?.Kill();
            }
            catch { /* Ignore */ }
        }
    }
}
