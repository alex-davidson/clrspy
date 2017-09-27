using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClrSpy.Architecture;

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

        public bool HasStarted => Process != null;
        public bool IsRunning => Process?.HasExited == false;

        public Process Start()
        {
            if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
            {
                return Startx86();
            }
            return StartNative();
        }

        protected Process StartNative()
        {
            if (HasStarted) throw new InvalidOperationException();

            var startInfo  = new ProcessStartInfo {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = processAssemblyPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = String.Join(" ", GetArguments().Select(CommandLineUtils.QuoteArgument))
            };
            return Process = Process.Start(startInfo);
        }

        protected Process Startx86()
        {
            if (HasStarted) throw new InvalidOperationException();

            var startInfo  = new ProcessStartInfo {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = x86Thunk.Bootstrap.GetThunkPath(),
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = String.Join(" ", new [] { processAssemblyPath }.Concat(GetArguments()).Select(CommandLineUtils.QuoteArgument))
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
