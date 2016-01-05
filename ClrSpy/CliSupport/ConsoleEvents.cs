using System;
using System.Threading;

namespace ClrSpy.CliSupport
{
    public class ConsoleEvents : ICancellationEvents
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public ConsoleEvents()
        {
            Console.CancelKeyPress += RequestCancel;
            ForcedTerminationMonitor.UrgentTerminationRequested += r => {
                cts.Cancel();
                KillRequested(null, r);
            };
        }

        public CancellationToken Token => cts.Token;
        public EventHandler<EventArgs> CancelRequested = (s, e) => { };
        public EventHandler<TerminationReason> KillRequested = (s, e) => { };

        public void LogRequestsTo(ConsoleLog log)
        {
            CancelRequested += (s, e) => log.WriteLine("Shutting down.");
            KillRequested += (s, e) => {
                log.WriteLine(e == TerminationReason.ControlBreak   ? "CTRL-Break pressed. Terminating."
                            : e == TerminationReason.ControlC       ? "CTRL-C pressed twice. Terminating."
                            : "Terminating.");
            };
        }
        
        public void RequestCancel(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                cts.Cancel();
                KillRequested(sender, TerminationReason.ControlBreak);
                return;
            }

            if (cts.IsCancellationRequested)
            {
                KillRequested(sender, TerminationReason.ControlC);
                return;
            }
            args.Cancel = true;
            cts.Cancel();
            CancelRequested(sender, args);
        }

        public void RegisterForDisposal(IDisposable resource)
        {
        }

        public void CheckForCancel()
        {
            if (!cts.IsCancellationRequested) return;

            throw new ErrorWithExitCodeException(255, "Operation was aborted via Ctrl-C.");
        }

        public bool WaitForCancel(TimeSpan timeout)
        {
            return Token.WaitHandle.WaitOne(timeout);
        }

        public void WaitForCancel()
        {
            Token.WaitHandle.WaitOne();
        }
    }
}