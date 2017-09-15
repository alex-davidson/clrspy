using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClrSpy.UnitTests.Utils
{
    public abstract class ProcessWrapper<TMain> : IDisposable
    {
        protected Process Process { get; private set; }
        private readonly string processAssemblyPath;

        protected ProcessWrapper()
        {
            var processAssembly = typeof(TMain).Assembly;
            processAssemblyPath = processAssembly.Location;
            if (processAssemblyPath == null) throw new Exception($"Could not locate assembly {processAssembly.FullName}");
        }

        protected virtual IEnumerable<string> GetArguments()
        {
            yield break;
        }

        private string BuildQuotedArguments()
        {
            return String.Join(" ", GetArguments().Select(CommandLineUtils.QuoteArgument));
        }

        public bool HasStarted => Process != null;
        public bool IsRunning => Process?.HasExited == false;

        public Process Start()
        {
            if (HasStarted) throw new InvalidOperationException();

            var startInfo  = new ProcessStartInfo {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = processAssemblyPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = BuildQuotedArguments()
            };
            return Process = Process.Start(startInfo);
        }

        protected virtual void TryShutdown()
        {
        }

        public void Dispose()
        {
            TryShutdown();
            try
            {
                if (Process.WaitForExit(0)) return;
                Process.Refresh();
                Process?.Kill();
            }
            catch { /* Ignore */ }
        }
    }
}
