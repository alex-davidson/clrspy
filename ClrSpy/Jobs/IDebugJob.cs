using System.IO;
using ClrSpy.CliSupport;

namespace ClrSpy.Jobs
{
    public interface IDebugJob
    {
        void Run(TextWriter output, ConsoleLog console);
    }
}