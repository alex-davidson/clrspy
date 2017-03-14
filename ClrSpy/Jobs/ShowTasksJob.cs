using System.IO;
using ClrSpy.CliSupport;
using ClrSpy.Processes;

namespace ClrSpy.Jobs
{
    public class ShowTasksJob : IDebugJob
    {
        private readonly IDebugSessionTarget target;
        public int? Pid => (target as DebugRunningProcess)?.Process.Pid;

        public ShowTasksJob(IDebugSessionTarget target)
        {
            this.target = target;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            console.WriteLine("Not implemented.");
        }
    }
}
