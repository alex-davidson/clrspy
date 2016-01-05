using System;
using System.Threading;

namespace ClrSpy.CliSupport
{
    public interface ICancellationEvents
    {
        CancellationToken Token { get; }
        void LogRequestsTo(ConsoleLog log);
        void CheckForCancel();
        bool WaitForCancel(TimeSpan timeout);
        void WaitForCancel();
    }
}