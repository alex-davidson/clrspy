using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClrSpy.CliSupport
{
    /// <summary>
    /// If termination-time cleanup is desperately necessary, prevent termination until
    /// we've had time to do that cleanup.
    /// </summary>
    public static class ForcedTerminationMonitor
    {
        /// <summary>
        /// Prevent the shell from 'casually' closing this process until the object is disposed.
        /// Time limits apply to this, so shutdown should be swift.
        /// </summary>
        /// <returns></returns>
        public static IDisposable HoldOpen()
        {
            var id = default(Guid);
            try
            {
                Acquire(out id);
                return new Lease(id);
            }
            catch
            {
                Release(id);
                throw;
            }
        }
        
        /// <summary>
        /// Maximum length of time allowed for process shutdown to complete before control is yielded to
        /// the shell and termination is forced. Default: 5 seconds. Not currently modifiable.
        /// </summary>
        public static TimeSpan ShutdownTimeout { get; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Raised when Ctrl-Break is detected or the hosting console session is closing.
        /// </summary>
        public static event Action<TerminationReason> UrgentTerminationRequested = r => { };


        static ForcedTerminationMonitor()
        {
           SetConsoleCtrlHandler(Handler, true);
        }
        
        private static readonly object signal = new object();
        private static readonly HashSet<Guid> leases = new HashSet<Guid>();
        delegate bool ConsoleCtrlHandler(TerminationReason reason);
            
        private static void Acquire(out Guid id)
        {
            id = Guid.NewGuid();
            lock(signal)
            {
                Debug.Assert(leases.Add(id));
            }
        }

        private static void Release(Guid id)
        {
            lock(signal)
            {
                if(leases.Remove(id)) Monitor.PulseAll(signal);
            }
        }
        
        private static void WaitOnTermination()
        {
            var stopwatch = Stopwatch.StartNew();
            var timeoutMilliseconds = (long)ShutdownTimeout.TotalMilliseconds;
            lock(signal)
            {
                while(leases.Count > 0)
                {
                    var remaining = timeoutMilliseconds - stopwatch.ElapsedMilliseconds;
                    if(remaining <= 0) return;
                    Monitor.Wait(signal, (int)remaining);
                }
            }
        }

        private static bool Handler(TerminationReason reason)
        {
            switch(reason)
            {
                case TerminationReason.ControlBreak:
                case TerminationReason.CloseConsoleHost:
                    UrgentTerminationRequested(reason);
                    WaitOnTermination();
                    return true;

                default:
                    // We're only looking to handle 'console closed' or Ctrl-Break events here.
                    // Anything else should be handed off to .NET or the OS, eg. Ctrl-C is handled
                    // using standard .NET techniques.
                    return false;
            }
        }
        
        struct Lease : IDisposable
        {
            private readonly Guid id;

            public Lease(Guid id)
            {
                this.id = id;
            }

            public void Dispose()
            {
                Release(id);
            }
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, bool add);
    }
}