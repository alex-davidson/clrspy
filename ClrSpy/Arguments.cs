using System;
using System.Collections.Generic;
using System.Linq;
using ClrSpy.CliSupport;

namespace ClrSpy
{
    public class Arguments
    {
        public Arguments()
        {
            JobType = JobType.DumpStacks;
        }

        public bool Verbose { get; set; }
        public int? Pid { get; set; }
        public bool PauseTargetProcess { get; set; }
        public JobType JobType { get; set; }

        public void ParseRemaining(IEnumerable<string> args)
        {
            if(!Pid.HasValue)
            {
                var pidString = args.FirstOrDefault();
                int pid;
                if (!Int32.TryParse(pidString, out pid))
                {
                    throw new ErrorWithExitCodeException(1, "No process ID specified.") { ShowUsage = true };
                }
                Pid = pid;
            }
        }
    }
}